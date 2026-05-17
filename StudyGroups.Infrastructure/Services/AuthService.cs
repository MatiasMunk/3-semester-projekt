using Dapper;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;
using StudyGroups.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace StudyGroups.Infrastructure.Services;

public class AuthService(IDbConnectionFactory factory, ILogger<AuthService> logger) : IAuthService
{
    private readonly IDbConnectionFactory _factory = factory;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<Result<User>> RegisterAsync(string username, string password)
    {
        username = username.Trim();

        _logger.LogWarning("AUTH SERVICE RegisterAsync called. Username='{Username}', PasswordLength={PasswordLength}", username, password?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(username))
            return Result<User>.Failure("Username is required");

        if (string.IsNullOrWhiteSpace(password))
            return Result<User>.Failure("Password is required");

        using var conn = _factory.CreateConnection();

        var exists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE Username = @Username",
            new { Username = username });

        if (exists > 0)
        {
            _logger.LogWarning("AUTH SERVICE user already exists. Username='{Username}'", username);
            return Result<User>.Failure("User exists");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, 12);

        var sql = @"
        INSERT INTO Users (Username, PasswordHash)
        VALUES (@Username, @PasswordHash);

        SELECT CAST(SCOPE_IDENTITY() as int);
    ";

        var id = await conn.ExecuteScalarAsync<int>(sql, new
        {
            Username = username,
            PasswordHash = hashedPassword
        });

        _logger.LogWarning("AUTH SERVICE inserted user. UserId={UserId}, Username='{Username}'", id, username);

        return Result<User>.Success(new User
        {
            Id = id,
            Username = username
        });
    }

    public async Task<Result<User>> LoginAsync(string username, string password)
    {
        username = username.Trim();

        using var conn = await _factory.CreateOpenAsync();

        var user = await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username",
            new { Username = username });

        if (user == null)
            return Result<User>.Failure("Invalid credentials");

        bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!valid)
            return Result<User>.Failure("Invalid credentials");

        return Result<User>.Success(user);
    }
}