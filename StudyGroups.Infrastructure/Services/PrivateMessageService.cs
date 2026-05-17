using Dapper;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;
using StudyGroups.Infrastructure.Data;

namespace StudyGroups.Infrastructure.Services;

public class PrivateMessageService(IDbConnectionFactory connectionFactory) : IPrivateMessageService
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Result<IEnumerable<PrivateMessage>>> GetConversationAsync(int sessionId, int userId, int otherUserId)
    {
        if (userId <= 0 || otherUserId <= 0)
            return Result<IEnumerable<PrivateMessage>>.Failure("Invalid user");

        using var conn = await _connectionFactory.CreateOpenAsync();

        await conn.ExecuteAsync(@"
            UPDATE PrivateMessages
            SET IsRead = 1
            WHERE SessionId = @SessionId
              AND SenderUserId = @OtherUserId
              AND ReceiverUserId = @UserId
              AND IsRead = 0
        ", new { SessionId = sessionId, UserId = userId, OtherUserId = otherUserId });

        var messages = await conn.QueryAsync<PrivateMessage>(@"
            SELECT pm.Id,
                   pm.SessionId,
                   pm.SenderUserId,
                   sender.Username AS SenderUsername,
                   pm.ReceiverUserId,
                   receiver.Username AS ReceiverUsername,
                   pm.Message,
                   pm.IsRead,
                   pm.CreatedAt
            FROM PrivateMessages pm
            INNER JOIN Users sender ON sender.Id = pm.SenderUserId
            INNER JOIN Users receiver ON receiver.Id = pm.ReceiverUserId
            WHERE pm.SessionId = @SessionId
              AND ((pm.SenderUserId = @UserId AND pm.ReceiverUserId = @OtherUserId)
                OR (pm.SenderUserId = @OtherUserId AND pm.ReceiverUserId = @UserId))
            ORDER BY pm.CreatedAt ASC, pm.Id ASC
        ", new { SessionId = sessionId, UserId = userId, OtherUserId = otherUserId });

        return Result<IEnumerable<PrivateMessage>>.Success(messages);
    }

    public async Task<Result<IEnumerable<PrivateMessage>>> GetUnreadAsync(int sessionId, int userId)
    {
        if (sessionId <= 0)
            return Result<IEnumerable<PrivateMessage>>.Failure("Invalid session");

        if (userId <= 0)
            return Result<IEnumerable<PrivateMessage>>.Failure("Invalid user");

        using var conn = await _connectionFactory.CreateOpenAsync();

        var messages = await conn.QueryAsync<PrivateMessage>(@"
            SELECT pm.Id,
                   pm.SessionId,
                   pm.SenderUserId,
                   sender.Username AS SenderUsername,
                   pm.ReceiverUserId,
                   receiver.Username AS ReceiverUsername,
                   pm.Message,
                   pm.IsRead,
                   pm.CreatedAt
            FROM PrivateMessages pm
            INNER JOIN Users sender ON sender.Id = pm.SenderUserId
            INNER JOIN Users receiver ON receiver.Id = pm.ReceiverUserId
            WHERE pm.SessionId = @SessionId
              AND pm.ReceiverUserId = @UserId
              AND pm.IsRead = 0
            ORDER BY pm.CreatedAt DESC, pm.Id DESC
        ", new { SessionId = sessionId, UserId = userId });

        return Result<IEnumerable<PrivateMessage>>.Success(messages);
    }

    public async Task<Result<PrivateMessage>> SendAsync(PrivateMessage message)
    {
        message.Message = message.Message.Trim();

        if (message.SessionId <= 0)
            return Result<PrivateMessage>.Failure("Invalid session");

        if (message.SenderUserId <= 0 || message.ReceiverUserId <= 0)
            return Result<PrivateMessage>.Failure("Invalid user");

        if (message.SenderUserId == message.ReceiverUserId)
            return Result<PrivateMessage>.Failure("Cannot send a private message to yourself");

        if (string.IsNullOrWhiteSpace(message.Message))
            return Result<PrivateMessage>.Failure("Message is required");

        using var conn = await _connectionFactory.CreateOpenAsync();

        var id = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO PrivateMessages (SessionId, SenderUserId, ReceiverUserId, Message, IsRead)
            VALUES (@SessionId, @SenderUserId, @ReceiverUserId, @Message, 0);

            SELECT CAST(SCOPE_IDENTITY() as int);
        ", message);

        var saved = await conn.QuerySingleAsync<PrivateMessage>(@"
            SELECT pm.Id,
                   pm.SessionId,
                   pm.SenderUserId,
                   sender.Username AS SenderUsername,
                   pm.ReceiverUserId,
                   receiver.Username AS ReceiverUsername,
                   pm.Message,
                   pm.IsRead,
                   pm.CreatedAt
            FROM PrivateMessages pm
            INNER JOIN Users sender ON sender.Id = pm.SenderUserId
            INNER JOIN Users receiver ON receiver.Id = pm.ReceiverUserId
            WHERE pm.Id = @Id
        ", new { Id = id });

        return Result<PrivateMessage>.Success(saved);
    }
}
