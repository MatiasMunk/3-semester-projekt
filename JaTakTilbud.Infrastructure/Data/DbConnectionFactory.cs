using Microsoft.Data.SqlClient;

namespace JaTakTilbud.Infrastructure.Data;

/// <summary>
/// Factory responsible for creating SQL connections.
/// Provides both raw and opened (async) connections.
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates a new SqlConnection (NOT opened).
    /// </summary>
    public SqlConnection Create()
    {
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Creates and opens a SqlConnection asynchronously.
    /// Preferred for async service methods.
    /// </summary>
    public async Task<SqlConnection> CreateOpenAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}