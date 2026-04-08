namespace OracleToMarkdown.Models;

/// <summary>
/// Represents an Oracle database table with its columns and metadata
/// </summary>
public class TableSchema
{
    public string TableName { get; set; } = string.Empty;
    public string? TableComments { get; set; }
    public List<ColumnSchema> Columns { get; set; } = new();

    /// <summary>
    /// Gets all foreign key columns in this table
    /// </summary>
    public List<ColumnSchema> GetForeignKeyColumns()
    {
        return Columns.Where(c => c.ConstraintType == "R").ToList();
    }

    /// <summary>
    /// Gets the primary key column(s) for this table
    /// </summary>
    public List<ColumnSchema> GetPrimaryKeyColumns()
    {
        return Columns.Where(c => c.ConstraintType == "P").ToList();
    }
}
