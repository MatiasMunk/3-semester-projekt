using Dapper;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;
using StudyGroups.Infrastructure.Data;

namespace StudyGroups.Infrastructure.Services;

public class UserService(IDbConnectionFactory connectionFactory) : IUserService
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var users = await conn.QueryAsync<User>(@"
            SELECT Id,
                   Username,
                   COALESCE(Email, '') AS Email,
                   CreatedAt
            FROM Users
            ORDER BY Username
        ");

        return Result<IEnumerable<User>>.Success(users);
    }
}
