using Microsoft.Data.SqlClient;
using System.Data;

namespace StudyGroups.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<SqlConnection> CreateOpenAsync();
}