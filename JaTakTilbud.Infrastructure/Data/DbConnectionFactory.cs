using Microsoft.Data.SqlClient;
using System.Data;

namespace JaTakTilbud.Infrastructure.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create()
    {
        return new SqlConnection(_connectionString);
    }
}