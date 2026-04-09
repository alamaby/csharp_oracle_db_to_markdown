namespace OracleToMarkdown.Config;

/// <summary>
/// Strongly typed configuration for Oracle database connection
/// </summary>
public class DatabaseConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1521;
    public string ServiceName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Builds the Oracle connection string from configuration
    /// </summary>
    public string GetConnectionString()
    {
        return $"User Id={UserId};Password={Password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={Port}))(CONNECT_DATA=(SERVICE_NAME={ServiceName})));";
    }

    /// <summary>
    /// Validates that all required configuration values are present
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Host))
            errors.Add("ORACLE_HOST is required");

        if (Port <= 0 || Port > 65535)
            errors.Add("ORACLE_PORT must be between 1 and 65535");

        if (string.IsNullOrWhiteSpace(ServiceName))
            errors.Add("ORACLE_SERVICE_NAME is required");

        if (string.IsNullOrWhiteSpace(UserId))
            errors.Add("ORACLE_USER_ID is required");

        if (string.IsNullOrWhiteSpace(Password))
            errors.Add("ORACLE_PASSWORD is required");

        return errors.Count == 0;
    }
}
