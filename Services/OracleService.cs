using Oracle.ManagedDataAccess.Client;
using OracleToMarkdown.Models;

namespace OracleToMarkdown.Services;

/// <summary>
/// Service responsible for fetching Oracle database metadata and schema information
/// </summary>
public class OracleService
{
    private readonly string _connectionString;

    public OracleService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Fetches complete schema information for all user tables
    /// </summary>
    public async Task<List<TableSchema>> FetchAllTableSchemasAsync()
    {
        var tables = new List<TableSchema>();

        try
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            Console.WriteLine("✓ Successfully connected to Oracle database");

            // Fetch table names first
            var tableNames = await GetTableNamesAsync(connection);
            Console.WriteLine($"✓ Found {tableNames.Count} tables");

            // For each table, fetch detailed schema
            foreach (var tableName in tableNames)
            {
                Console.WriteLine($"  Processing table: {tableName}");
                var tableSchema = await GetTableSchemaAsync(connection, tableName);
                tables.Add(tableSchema);
            }
        }
        catch (OracleException ex)
        {
            Console.Error.WriteLine($"✗ Oracle Database Error: {ex.Message}");
            Console.Error.WriteLine($"   Error Code: {ex.Number}");
            throw;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Unexpected Error: {ex.Message}");
            throw;
        }

        return tables;
    }

    /// <summary>
    /// Gets all user table names from the database
    /// </summary>
    private async Task<List<string>> GetTableNamesAsync(OracleConnection connection)
    {
        var tables = new List<string>();
        
        // Get tables that have columns
        var query = @"
            SELECT DISTINCT table_name 
            FROM user_tab_columns 
            WHERE table_name NOT LIKE 'BIN$%' 
            ORDER BY table_name";

        using var command = new OracleCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    /// <summary>
    /// Gets complete schema information for a specific table
    /// </summary>
    private async Task<TableSchema> GetTableSchemaAsync(OracleConnection connection, string tableName)
    {
        var tableSchema = new TableSchema
        {
            TableName = tableName,
            TableComments = await GetTableCommentsAsync(connection, tableName)
        };

        // Get column information
        tableSchema.Columns = await GetColumnSchemaAsync(connection, tableName);

        return tableSchema;
    }

    /// <summary>
    /// Gets table comments/description
    /// </summary>
    private async Task<string?> GetTableCommentsAsync(OracleConnection connection, string tableName)
    {
        var query = @"
            SELECT comments 
            FROM user_tab_comments 
            WHERE table_name = :tableName";

        using var command = new OracleCommand(query, connection);
        command.Parameters.Add(new OracleParameter("tableName", tableName));

        var result = await command.ExecuteScalarAsync();
        return result?.ToString();
    }

    /// <summary>
    /// Gets detailed column information for a specific table
    /// </summary>
    private async Task<List<ColumnSchema>> GetColumnSchemaAsync(OracleConnection connection, string tableName)
    {
        var columns = new Dictionary<string, ColumnSchema>();

        // Query 1: Get basic column information
        var columnQuery = @"
            SELECT 
                column_name,
                data_type,
                data_length,
                data_precision,
                data_scale,
                nullable
            FROM user_tab_columns
            WHERE table_name = :tableName
            ORDER BY column_id";

        using (var cmd = new OracleCommand(columnQuery, connection))
        {
            cmd.Parameters.Add(new OracleParameter("tableName", tableName));
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var columnName = reader.GetString(0);
                columns[columnName] = new ColumnSchema
                {
                    ColumnName = columnName,
                    DataType = reader.GetString(1),
                    DataLength = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                    DataPrecision = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                    DataScale = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                    IsNullable = reader.GetString(5) == "Y"
                };
            }
        }

        // Query 2: Get column comments
        var commentQuery = @"
            SELECT column_name, comments
            FROM user_col_comments
            WHERE table_name = :tableName";

        using (var cmd = new OracleCommand(commentQuery, connection))
        {
            cmd.Parameters.Add(new OracleParameter("tableName", tableName));
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var columnName = reader.GetString(0);
                if (columns.ContainsKey(columnName))
                {
                    var comments = reader.IsDBNull(1) ? null : reader.GetString(1);
                    columns[columnName].Comments = comments;
                }
            }
        }

        // Query 3: Get constraints (PK and FK)
        var constraintQuery = @"
            SELECT 
                uc.constraint_type,
                ucc.column_name,
                rcc.table_name AS referenced_table,
                rcc.column_name AS referenced_column
            FROM user_constraints uc
            LEFT JOIN user_cons_columns ucc 
                ON uc.constraint_name = ucc.constraint_name 
                AND uc.table_name = ucc.table_name
            LEFT JOIN user_constraints rc 
                ON uc.r_constraint_name = rc.constraint_name
            LEFT JOIN user_cons_columns rcc 
                ON rc.constraint_name = rcc.constraint_name
                AND rc.table_name = rcc.table_name
            WHERE uc.table_name = :tableName
            AND uc.constraint_type IN ('P', 'R')
            ORDER BY uc.constraint_type, ucc.position";

        using (var cmd = new OracleCommand(constraintQuery, connection))
        {
            cmd.Parameters.Add(new OracleParameter("tableName", tableName));
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var constraintType = reader.GetString(0);
                var columnName = reader.IsDBNull(1) ? null : reader.GetString(1);
                var refTable = reader.IsDBNull(2) ? null : reader.GetString(2);
                var refColumn = reader.IsDBNull(3) ? null : reader.GetString(3);

                if (columnName != null && columns.ContainsKey(columnName))
                {
                    columns[columnName].ConstraintType = constraintType;
                    columns[columnName].ReferencedTable = refTable;
                    columns[columnName].ReferencedColumn = refColumn;
                }
            }
        }

        return columns.Values.ToList();
    }

    /// <summary>
    /// Validates the database connection
    /// </summary>
    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new OracleCommand("SELECT 1 FROM DUAL", connection);
            var result = await command.ExecuteScalarAsync();
            
            return result != null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Connection validation failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the current Oracle schema/user name
    /// </summary>
    public async Task<string?> GetCurrentSchemaAsync()
    {
        try
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new OracleCommand("SELECT SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') FROM DUAL", connection);
            var result = await command.ExecuteScalarAsync();
            
            return result?.ToString();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Failed to get schema name: {ex.Message}");
            return null;
        }
    }
}
