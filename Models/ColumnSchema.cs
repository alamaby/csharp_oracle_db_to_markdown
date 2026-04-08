namespace OracleToMarkdown.Models;

/// <summary>
/// Represents a column in an Oracle database table with all metadata
/// </summary>
public class ColumnSchema
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? DataLength { get; set; }
    public int? DataPrecision { get; set; }
    public int? DataScale { get; set; }
    public bool IsNullable { get; set; }
    public string? Comments { get; set; }
    public string? ConstraintType { get; set; } // PK, FK, or null
    public string? ReferencedTable { get; set; }
    public string? ReferencedColumn { get; set; }

    /// <summary>
    /// Returns the full data type string with precision/scale if applicable
    /// </summary>
    public string GetFullDataType()
    {
        var baseType = DataType;
        
        if (DataType == "NUMBER" && DataPrecision.HasValue)
        {
            return DataScale.HasValue && DataScale.Value > 0 
                ? $"{DataType}({DataPrecision.Value},{DataScale.Value})" 
                : $"{DataType}({DataPrecision.Value})";
        }
        
        if (DataType == "VARCHAR2" && DataLength.HasValue)
        {
            return $"{DataType}({DataLength.Value})";
        }
        
        if (DataType == "NVARCHAR2" && DataLength.HasValue)
        {
            return $"{DataType}({DataLength.Value})";
        }
        
        if (DataType == "CHAR" && DataLength.HasValue)
        {
            return $"{DataType}({DataLength.Value})";
        }
        
        return baseType;
    }
}
