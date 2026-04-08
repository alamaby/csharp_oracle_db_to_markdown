using OracleToMarkdown.Models;

namespace OracleToMarkdown.Services;

/// <summary>
/// Service responsible for generating Markdown documentation from Oracle schema data
/// </summary>
public class MarkdownGeneratorService
{
    /// <summary>
    /// Generates a complete Markdown data dictionary document
    /// </summary>
    public string GenerateMarkdown(List<TableSchema> tables, string outputPath)
    {
        var markdown = new System.Text.StringBuilder();

        // Generate header and summary
        markdown.AppendLine("# Data Dictionary");
        markdown.AppendLine();
        markdown.AppendLine($"> **Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        markdown.AppendLine();
        markdown.AppendLine("## Summary");
        markdown.AppendLine();
        markdown.AppendLine($"- **Total Tables:** {tables.Count}");
        markdown.AppendLine($"- **Total Columns:** {tables.Sum(t => t.Columns.Count)}");
        markdown.AppendLine($"- **Tables with Foreign Keys:** {tables.Count(t => t.GetForeignKeyColumns().Any())}");
        markdown.AppendLine();

        // Generate Mermaid ER Diagram
        markdown.AppendLine("## Entity Relationship Diagram");
        markdown.AppendLine();
        markdown.AppendLine("```mermaid");
        markdown.AppendLine("erDiagram");
        GenerateMermaidERD(markdown, tables);
        markdown.AppendLine("```");
        markdown.AppendLine();

        // Generate table documentation
        markdown.AppendLine("## Table Documentation");
        markdown.AppendLine();

        foreach (var table in tables)
        {
            GenerateTableDocumentation(markdown, table);
        }

        // Generate index
        markdown.AppendLine("---");
        markdown.AppendLine();
        markdown.AppendLine("## Quick Reference Index");
        markdown.AppendLine();
        
        foreach (var table in tables.OrderBy(t => t.TableName))
        {
            var anchor = table.TableName.ToLower().Replace("_", "-");
            markdown.AppendLine($"- [{table.TableName}](#{anchor})");
        }
        
        markdown.AppendLine();

        return markdown.ToString();
    }

    /// <summary>
    /// Generates Mermaid ERD syntax for all tables and their relationships
    /// </summary>
    private void GenerateMermaidERD(System.Text.StringBuilder markdown, List<TableSchema> tables)
    {
        // First pass: define all entities
        foreach (var table in tables)
        {
            markdown.AppendLine($"    {table.TableName} {{");
            
            foreach (var column in table.Columns)
            {
                var type = column.GetFullDataType();
                var isPK = column.ConstraintType == "P";
                var isFK = column.ConstraintType == "R";
                
                var notation = "";
                if (isPK) notation = " PK";
                else if (isFK) notation = " FK";
                
                markdown.AppendLine($"        {type} {column.ColumnName}{notation}");
            }
            
            markdown.AppendLine("    }");
            markdown.AppendLine();
        }

        // Second pass: define relationships
        foreach (var table in tables)
        {
            var fkColumns = table.GetForeignKeyColumns();
            
            foreach (var fkColumn in fkColumns)
            {
                if (!string.IsNullOrEmpty(fkColumn.ReferencedTable))
                {
                    // Mermaid syntax: TABLE_NAME ||--o{ REFERENCED_TABLE : "column_name"
                    markdown.AppendLine($"    {table.TableName} }}o--|| {fkColumn.ReferencedTable} : \"{fkColumn.ColumnName}\"");
                }
            }
        }
    }

    /// <summary>
    /// Generates detailed documentation for a single table
    /// </summary>
    private void GenerateTableDocumentation(System.Text.StringBuilder markdown, TableSchema table)
    {
        var anchor = table.TableName.ToLower().Replace("_", "-");
        
        // Table header
        markdown.AppendLine($"## {table.TableName}");
        markdown.AppendLine();
        markdown.AppendLine($"<a id=\"{anchor}\"></a>");
        markdown.AppendLine();

        // Table description if available
        if (!string.IsNullOrEmpty(table.TableComments))
        {
            markdown.AppendLine($"> {table.TableComments}");
            markdown.AppendLine();
        }

        // Table metadata
        markdown.AppendLine("**Metadata:**");
        markdown.AppendLine();
        markdown.AppendLine($"- **Columns:** {table.Columns.Count}");
        markdown.AppendLine($"- **Primary Key:** {string.Join(", ", table.GetPrimaryKeyColumns().Select(c => $"`{c.ColumnName}`"))}");
        
        var fkColumns = table.GetForeignKeyColumns();
        if (fkColumns.Any())
        {
            var fkRelations = fkColumns
                .Where(c => !string.IsNullOrEmpty(c.ReferencedTable))
                .Select(c => $"`{c.ColumnName}` → `{c.ReferencedTable}.{c.ReferencedColumn}`");
            
            markdown.AppendLine($"- **Foreign Keys:** {string.Join(", ", fkRelations)}");
        }
        
        markdown.AppendLine();

        // Column documentation table
        markdown.AppendLine("### Columns");
        markdown.AppendLine();
        markdown.AppendLine("| # | Column Name | Data Type | Nullable | Constraint | Description |");
        markdown.AppendLine("|---|-------------|-----------|----------|------------|-------------|");

        for (int i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            var nullable = column.IsNullable ? "✓" : "✗";
            var constraint = GetConstraintSymbol(column);
            var description = EscapeMarkdown(column.Comments ?? "—");
            
            markdown.AppendLine($"| {i + 1} | `{column.ColumnName}` | {column.GetFullDataType()} | {nullable} | {constraint} | {description} |");
        }

        markdown.AppendLine();

        // Sample relationships if FK exists
        if (fkColumns.Any())
        {
            markdown.AppendLine("### Relationships");
            markdown.AppendLine();
            
            foreach (var fk in fkColumns.Where(c => !string.IsNullOrEmpty(c.ReferencedTable)))
            {
                markdown.AppendLine($"- **{fk.ColumnName}** references [{fk.ReferencedTable}](#{fk.ReferencedTable.ToLower().Replace("_", "-")}).{fk.ReferencedColumn}");
            }
            
            markdown.AppendLine();
        }

        markdown.AppendLine("---");
        markdown.AppendLine();
    }

    /// <summary>
    /// Returns constraint symbol (🔑 for PK, 🔗 for FK, or empty)
    /// </summary>
    private string GetConstraintSymbol(ColumnSchema column)
    {
        return column.ConstraintType switch
        {
            "P" => "🔑 PK",
            "R" => "🔗 FK",
            _ => "—",
        };
    }

    /// <summary>
    /// Escapes special Markdown characters in text
    /// </summary>
    private string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "—";

        // Escape pipe characters which break tables
        return text.Replace("|", "\\|");
    }

    /// <summary>
    /// Writes the generated Markdown to a file
    /// </summary>
    public async Task WriteToFileAsync(string content, string outputPath)
    {
        await File.WriteAllTextAsync(outputPath, content);
        Console.WriteLine($"✓ Data dictionary written to: {Path.GetFullPath(outputPath)}");
    }
}
