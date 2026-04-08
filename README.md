# Oracle-to-Markdown Data Dictionary Generator

A powerful CLI tool that automatically generates comprehensive data dictionary documentation from Oracle 19c databases. Built with .NET 8 and C#, this tool bridges the gap between database schemas and developer-friendly documentation.

![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Oracle](https://img.shields.io/badge/Oracle-19c-red)

## 🎯 Why This Tool Matters for System Analysts

In the **Software Development Life Cycle (SDLC)**, maintaining accurate and up-to-date database documentation is critical:

### 📋 Documentation Automation
- **Eliminates Manual Effort**: No more hours spent writing table/column descriptions by hand
- **Always Up-to-Date**: Generate fresh documentation whenever schema changes
- **Consistent Format**: Standardized markdown output ensures uniform documentation quality

### 🔍 Reverse Engineering & Analysis
- **Legacy System Understanding**: Quickly comprehend unknown database structures
- **Impact Analysis**: Understand table relationships before making schema changes
- **Code Review Support**: Verify database changes match design specifications

### 🤝 Team Collaboration & Knowledge Transfer
- **Onboarding Acceleration**: New team members can understand the data model quickly
- **Single Source of Truth**: Share `DATA_DICTIONARY.md` across the team
- **Version Control Friendly**: Markdown files diff nicely in Git

### 🏗️ Architecture & Design
- **ERD Visualization**: Mermaid.js diagrams provide visual relationship mapping
- **Design Validation**: Compare actual schema against architectural design documents
- **API Development**: Understand data structures before building REST/GraphQL endpoints

---

## 🚀 Features

- ✅ **Automatic Schema Discovery**: Reads all user tables, columns, and constraints
- ✅ **Rich Metadata Extraction**:
  - Table names and descriptions
  - Column names, data types (with precision/scale)
  - Nullability constraints
  - Primary and Foreign key relationships
  - Column comments/descriptions
- ✅ **Mermaid.js Entity Relationship Diagram**: Visual table relationship mapping
- ✅ **Scannable Markdown Tables**: Professional, readable output
- ✅ **Environment-Based Configuration**: Secure credential management via `.env`
- ✅ **Error Handling**: Graceful failure with helpful error messages
- ✅ **Cross-Platform**: Works on Windows, macOS, and Linux

---

## 📦 Installation

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Access to an Oracle 19c database
- Oracle client libraries (included via `Oracle.ManagedDataAccess.Core`)

### Setup Steps

1. **Clone or download this repository**
   ```bash
   git clone <repository-url>
   cd OracleToMarkdown
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build --configuration Release
   ```

---

## ⚙️ Configuration

### Setting Up Your `.env` File

1. **Copy the example configuration**
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env` with your Oracle database credentials**
   ```env
   # Oracle Database Connection Configuration
   ORACLE_HOST=localhost
   ORACLE_PORT=1521
   ORACLE_SERVICE_NAME=ORCL
   ORACLE_USER_ID=your_username
   ORACLE_PASSWORD=your_password
   ```

   **Configuration Parameters:**
   | Variable | Description | Example |
   |----------|-------------|---------|
   | `ORACLE_HOST` | Database server hostname | `db.example.com` |
   | `ORACLE_PORT` | Oracle listener port (default: 1521) | `1521` |
   | `ORACLE_SERVICE_NAME` | Oracle service name or SID | `ORCLPDB1` |
   | `ORACLE_USER_ID` | Database user with SELECT access | `SCHEMA_OWNER` |
   | `ORACLE_PASSWORD` | Database user password | `MySecurePass123!` |

3. **⚠️ Security Warning**: Never commit `.env` to version control! The `.gitignore` file is pre-configured to exclude it.

---

## 🎮 Usage

### Basic Usage

Run the tool from the command line:

```bash
dotnet run
```

Or execute the compiled binary:

```bash
dotnet run --project OracleToMarkdown/OracleToMarkdown.csproj
```

### What Happens

The tool will:

1. ✅ Load configuration from `.env`
2. ✅ Connect to Oracle database
3. ✅ Discover all user tables
4. ✅ Extract schema metadata (columns, types, constraints, comments)
5. ✅ Generate `DATA_DICTIONARY.md` with:
   - Summary statistics
   - Mermaid.js Entity Relationship Diagram
   - Detailed table documentation with column specifications
   - Quick reference index

### Output Example

After successful execution, you'll find `DATA_DICTIONARY.md` in the project root:

```markdown
# Data Dictionary

> **Generated:** 2026-04-08 14:30:00

## Summary

- **Total Tables:** 5
- **Total Columns:** 47
- **Tables with Foreign Keys:** 4

## Entity Relationship Diagram

```mermaid
erDiagram
    USERS {
        NUMBER USER_ID PK
        VARCHAR2(100) USERNAME
        ...
    }
    
    ORDERS }o--|| USERS : "USER_ID"
```

## Table Documentation

### USERS

> System users and authentication

**Metadata:**

- **Columns:** 8
- **Primary Key:** `USER_ID`

### Columns

| # | Column Name | Data Type | Nullable | Constraint | Description |
|---|-------------|-----------|----------|------------|-------------|
| 1 | `USER_ID` | NUMBER(10) | ✗ | 🔑 PK | Unique user identifier |
| 2 | `USERNAME` | VARCHAR2(100) | ✗ | — | Login username |
...
```

---

## 🏗️ Architecture

This project follows clean architecture principles:

```
OracleToMarkdown/
├── Config/
│   └── DatabaseConfig.cs          # Strongly typed configuration
├── Models/
│   ├── ColumnSchema.cs            # Column metadata model
│   └── TableSchema.cs             # Table metadata model
├── Services/
│   ├── OracleService.cs           # Database access and metadata extraction
│   └── MarkdownGeneratorService.cs # Markdown documentation generation
├── Program.cs                     # Application entry point
├── .env.example                   # Configuration template
└── OracleToMarkdown.csproj        # Project file with dependencies
```

### Design Patterns Used

- **Service Pattern**: Separation of concerns between data access (`OracleService`) and output generation (`MarkdownGeneratorService`)
- **Dependency Injection**: Clean service instantiation in `Program.cs`
- **Strongly Typed Models**: `TableSchema` and `ColumnSchema` for type safety
- **Factory Pattern**: Static configuration loading with validation

---

## 📚 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| [DotNetEnv](https://github.com/tonerdo/dotnet-env) | 3.0.0 | `.env` file loading and parsing |
| [Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core) | 23.4.0 | Oracle database connectivity (fully managed, no Oracle client required) |

---

## 🔧 Troubleshooting

### Common Issues

**1. "Configuration file '.env' not found"**
   - **Solution**: Copy `.env.example` to `.env` and fill in your credentials

**2. "Oracle Database Error: ORA-01017: invalid username/password"**
   - **Solution**: Verify your `ORACLE_USER_ID` and `ORACLE_PASSWORD` in `.env`

**3. "Oracle Database Error: ORA-12541: TNS:no listener"**
   - **Solution**: Check `ORACLE_HOST`, `ORACLE_PORT`, and ensure Oracle is running

**4. "ORA-12154: TNS:could not resolve the connect identifier"**
   - **Solution**: Verify `ORACLE_SERVICE_NAME` matches your Oracle configuration

**5. Build fails with missing package errors**
   - **Solution**: Run `dotnet restore` to download NuGet packages

### Getting Help

If you encounter issues not covered here:

1. Check the error message details
2. Verify your `.env` configuration
3. Test database connectivity with SQL Developer or similar tool
4. Open an issue with error details (remove sensitive info first!)

---

## 🧪 Development

### Adding New Features

```bash
# Watch mode for auto-rebuild during development
dotnet watch run

# Build in debug mode
dotnet build

# Run tests (when added)
dotnet test
```

### Code Quality

The codebase follows these principles:
- XML documentation comments on all public members
- Async/await for all I/O operations
- Proper resource disposal with `using` statements
- Strong typing throughout
- Clean separation of concerns

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🤝 Contributing

Contributions are welcome! Whether it's:

- Bug fixes
- New features (e.g., additional output formats)
- Documentation improvements
- Performance optimizations

Please feel free to submit a Pull Request!

---

## 👨‍💻 Author

Built with ❤️ for System Analysts and developers who value good documentation.

**GitHub**: [alamaby](https://github.com/alamaby)

---

## 📊 Sample Output

See [DATA_DICTIONARY.md](DATA_DICTIONARY.md) (generated after running against your database) for a complete example of the generated documentation.

---

**Happy documenting! 📚✨**
