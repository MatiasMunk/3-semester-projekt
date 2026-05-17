using Dapper;
using Microsoft.Data.SqlClient;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;
using StudyGroups.Infrastructure.Data;

namespace StudyGroups.Infrastructure.Services;

public class FriendRequestService(IDbConnectionFactory connectionFactory) : IFriendRequestService
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Result<IEnumerable<FriendRequest>>> GetPendingIncomingAsync(int receiverUserId)
    {
        if (receiverUserId <= 0)
            return Result<IEnumerable<FriendRequest>>.Failure("Invalid user");

        using var conn = await _connectionFactory.CreateOpenAsync();

        var requests = await conn.QueryAsync<FriendRequest>(@"
            SELECT fr.Id,
                   fr.RequesterUserId,
                   requester.Username AS RequesterUsername,
                   fr.ReceiverUserId,
                   receiver.Username AS ReceiverUsername,
                   fr.Status,
                   fr.CreatedAt,
                   fr.RespondedAt
            FROM FriendRequests fr
            INNER JOIN Users requester ON requester.Id = fr.RequesterUserId
            INNER JOIN Users receiver ON receiver.Id = fr.ReceiverUserId
            WHERE fr.ReceiverUserId = @ReceiverUserId
              AND fr.Status = N'pending'
              AND NOT EXISTS (
                    SELECT 1
                    FROM Friendships fs
                    WHERE fs.UserId = fr.ReceiverUserId
                      AND fs.FriendUserId = fr.RequesterUserId
              )
            ORDER BY fr.CreatedAt DESC, fr.Id DESC
        ", new { ReceiverUserId = receiverUserId });

        return Result<IEnumerable<FriendRequest>>.Success(requests);
    }

    public async Task<Result<IEnumerable<int>>> GetFriendIdsAsync(int userId)
    {
        if (userId <= 0)
            return Result<IEnumerable<int>>.Failure("Invalid user");

        using var conn = await _connectionFactory.CreateOpenAsync();

        var ids = await conn.QueryAsync<int>(@"
            SELECT FriendUserId
            FROM Friendships
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC, Id DESC
        ", new { UserId = userId });

        return Result<IEnumerable<int>>.Success(ids);
    }

    public async Task<Result<FriendRequest>> CreateAsync(int requesterUserId, int receiverUserId)
    {
        if (requesterUserId <= 0 || receiverUserId <= 0)
            return Result<FriendRequest>.Failure("Invalid user");

        if (requesterUserId == receiverUserId)
            return Result<FriendRequest>.Failure("Cannot send a friend request to yourself");

        using var conn = await _connectionFactory.CreateOpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            var alreadyFriends = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM Friendships
                WHERE UserId = @RequesterUserId AND FriendUserId = @ReceiverUserId
            ", new { RequesterUserId = requesterUserId, ReceiverUserId = receiverUserId }, transaction);

            if (alreadyFriends > 0)
            {
                transaction.Rollback();
                return Result<FriendRequest>.Failure("Users are already friends");
            }

            var sameDirection = await conn.QuerySingleOrDefaultAsync<FriendRequest>(@"
                SELECT TOP 1 Id, RequesterUserId, ReceiverUserId, Status, CreatedAt, RespondedAt
                FROM FriendRequests
                WHERE RequesterUserId = @RequesterUserId
                  AND ReceiverUserId = @ReceiverUserId
                ORDER BY CreatedAt DESC, Id DESC
            ", new { RequesterUserId = requesterUserId, ReceiverUserId = receiverUserId }, transaction);

            if (sameDirection != null)
            {
                if (string.Equals(sameDirection.Status, "pending", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Commit();
                    return Result<FriendRequest>.Success(await GetByIdAsync(conn, sameDirection.Id));
                }

                // Re-adding after a removed friendship must create a fresh pending request,
                // not reuse old accepted/declined history as an automatic acceptance.
                await conn.ExecuteAsync(@"
                    UPDATE FriendRequests
                    SET Status = N'pending',
                        RespondedAt = NULL,
                        CreatedAt = GETDATE()
                    WHERE Id = @Id
                ", new { sameDirection.Id }, transaction);

                transaction.Commit();
                return Result<FriendRequest>.Success(await GetByIdAsync(conn, sameDirection.Id));
            }

            var reversePending = await conn.QuerySingleOrDefaultAsync<FriendRequest>(@"
                SELECT TOP 1 Id, RequesterUserId, ReceiverUserId, Status, CreatedAt, RespondedAt
                FROM FriendRequests
                WHERE RequesterUserId = @ReceiverUserId
                  AND ReceiverUserId = @RequesterUserId
                  AND Status = N'pending'
                ORDER BY CreatedAt DESC, Id DESC
            ", new { RequesterUserId = requesterUserId, ReceiverUserId = receiverUserId }, transaction);

            if (reversePending != null)
            {
                await AcceptRequestPairAsync(conn, transaction, reversePending.Id, reversePending.RequesterUserId, reversePending.ReceiverUserId);
                transaction.Commit();
                return Result<FriendRequest>.Success(await GetByIdAsync(conn, reversePending.Id));
            }

            var id = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO FriendRequests (RequesterUserId, ReceiverUserId, Status)
                VALUES (@RequesterUserId, @ReceiverUserId, N'pending');

                SELECT CAST(SCOPE_IDENTITY() as int);
            ", new { RequesterUserId = requesterUserId, ReceiverUserId = receiverUserId }, transaction);

            transaction.Commit();

            var request = await GetByIdAsync(conn, id);
            return Result<FriendRequest>.Success(request);
        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            transaction.Rollback();

            // Race-safe fallback: if another request won the insert, return pending history only.
            // Do not return old accepted history as success, because the client treats accepted as friendship.
            var existing = await conn.QueryFirstOrDefaultAsync<FriendRequest>(@"
                SELECT TOP 1 Id, RequesterUserId, ReceiverUserId, Status, CreatedAt, RespondedAt
                FROM FriendRequests
                WHERE RequesterUserId = @RequesterUserId
                  AND ReceiverUserId = @ReceiverUserId
                  AND Status = N'pending'
                ORDER BY CreatedAt DESC, Id DESC
            ", new { RequesterUserId = requesterUserId, ReceiverUserId = receiverUserId });

            return existing == null
                ? Result<FriendRequest>.Failure("Friend request already exists")
                : Result<FriendRequest>.Success(await GetByIdAsync(conn, existing.Id));
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return Result<FriendRequest>.Failure(ex.Message);
        }
    }

    public async Task<Result<FriendRequest>> RespondAsync(int requestId, int receiverUserId, bool accept)
    {
        if (requestId <= 0 || receiverUserId <= 0)
            return Result<FriendRequest>.Failure("Invalid friend request");

        using var conn = await _connectionFactory.CreateOpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            var request = await conn.QuerySingleOrDefaultAsync<FriendRequest>(@"
                SELECT Id, RequesterUserId, ReceiverUserId, Status, CreatedAt, RespondedAt
                FROM FriendRequests
                WHERE Id = @RequestId AND ReceiverUserId = @ReceiverUserId
            ", new { RequestId = requestId, ReceiverUserId = receiverUserId }, transaction);

            if (request == null)
            {
                transaction.Rollback();
                return Result<FriendRequest>.Failure("Friend request not found");
            }

            if (!string.Equals(request.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Rollback();
                return Result<FriendRequest>.Failure("Friend request already handled");
            }

            if (accept)
            {
                await AcceptRequestPairAsync(conn, transaction, request.Id, request.RequesterUserId, request.ReceiverUserId);
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE FriendRequests
                    SET Status = N'declined',
                        RespondedAt = GETDATE()
                    WHERE Id = @RequestId
                ", new { RequestId = requestId }, transaction);
            }

            transaction.Commit();

            var updated = await GetByIdAsync(conn, requestId);
            return Result<FriendRequest>.Success(updated);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return Result<FriendRequest>.Failure(ex.Message);
        }
    }

    public async Task<Result> RemoveFriendAsync(int userId, int friendUserId)
    {
        if (userId <= 0 || friendUserId <= 0)
            return Result.Failure("Invalid user");

        if (userId == friendUserId)
            return Result.Failure("Cannot remove yourself as a friend");

        using var conn = await _connectionFactory.CreateOpenAsync();
        using var transaction = conn.BeginTransaction();

        await conn.ExecuteAsync(@"
            DELETE FROM Friendships
            WHERE (UserId = @UserId AND FriendUserId = @FriendUserId)
               OR (UserId = @FriendUserId AND FriendUserId = @UserId)
        ", new { UserId = userId, FriendUserId = friendUserId }, transaction);

        // Clear old accepted request state after unfriend. This preserves history as declined,
        // and makes a future add require a fresh pending request + explicit acceptance.
        await conn.ExecuteAsync(@"
            UPDATE FriendRequests
            SET Status = N'declined',
                RespondedAt = GETDATE()
            WHERE ((RequesterUserId = @UserId AND ReceiverUserId = @FriendUserId)
                OR (RequesterUserId = @FriendUserId AND ReceiverUserId = @UserId))
              AND Status = N'accepted'
        ", new { UserId = userId, FriendUserId = friendUserId }, transaction);

        transaction.Commit();

        return Result.Success();
    }

    private static async Task AcceptRequestPairAsync(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, int acceptedRequestId, int requesterUserId, int receiverUserId)
    {
        await conn.ExecuteAsync(@"
            UPDATE FriendRequests
            SET Status = N'accepted',
                RespondedAt = COALESCE(RespondedAt, GETDATE())
            WHERE ((RequesterUserId = @RequesterUserId AND ReceiverUserId = @ReceiverUserId)
                OR (RequesterUserId = @ReceiverUserId AND ReceiverUserId = @RequesterUserId))
              AND Status = N'pending'
        ", new { RequesterUserId = requesterUserId, ReceiverUserId = receiverUserId }, transaction);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Friendships WHERE UserId = @ReceiverUserId AND FriendUserId = @RequesterUserId)
                INSERT INTO Friendships (UserId, FriendUserId) VALUES (@ReceiverUserId, @RequesterUserId);

            IF NOT EXISTS (SELECT 1 FROM Friendships WHERE UserId = @RequesterUserId AND FriendUserId = @ReceiverUserId)
                INSERT INTO Friendships (UserId, FriendUserId) VALUES (@RequesterUserId, @ReceiverUserId);
        ", new { RequesterUserId = requesterUserId, ReceiverUserId = receiverUserId }, transaction);
    }

    private static async Task<FriendRequest> GetByIdAsync(System.Data.IDbConnection conn, int id)
    {
        return await conn.QuerySingleAsync<FriendRequest>(@"
            SELECT fr.Id,
                   fr.RequesterUserId,
                   requester.Username AS RequesterUsername,
                   fr.ReceiverUserId,
                   receiver.Username AS ReceiverUsername,
                   fr.Status,
                   fr.CreatedAt,
                   fr.RespondedAt
            FROM FriendRequests fr
            INNER JOIN Users requester ON requester.Id = fr.RequesterUserId
            INNER JOIN Users receiver ON receiver.Id = fr.ReceiverUserId
            WHERE fr.Id = @Id
        ", new { Id = id });
    }
}
