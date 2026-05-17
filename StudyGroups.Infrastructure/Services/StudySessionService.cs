using Dapper;
using Microsoft.Data.SqlClient;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;
using StudyGroups.Infrastructure.Data;
using System.Data;

namespace StudyGroups.Infrastructure.Services;

public class StudySessionService : IStudySessionService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StudySessionService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // -----------------------------
    // GET ALL
    // -----------------------------
    public async Task<Result<IEnumerable<StudySession>>> GetAllAsync(string? category)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var sql = @"
            SELECT s.*, t.Id, t.Name, t.Icon, t.Color
            FROM StudySessions s
            INNER JOIN Topics t ON t.Id = s.TopicId
        ";

        if (!string.IsNullOrWhiteSpace(category))
        {
            sql += " WHERE LOWER(t.Name) = LOWER(@Category) OR LOWER(t.Slug) = LOWER(@Category)";
        }

        var sessions = await conn.QueryAsync<StudySession, Topic, StudySession>(
            sql,
            (session, topic) =>
            {
                session.Topic = topic;
                return session;
            },
            new { Category = category },
            splitOn: "Id"
        );

        return Result<IEnumerable<StudySession>>.Success(sessions);
    }

    // -----------------------------
    // GET BY ID (WITH PARTICIPANTS)
    // -----------------------------
    public async Task<Result<StudySession>> GetByIdAsync(int id)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var sql = @"
        SELECT 
            s.*,

            t.Id, t.Name, t.Icon, t.Color,

            sp.Id, sp.SessionId, sp.UserId, sp.JoinedAt,

            u.Id, u.Username

        FROM StudySessions s

        INNER JOIN Topics t ON t.Id = s.TopicId

        LEFT JOIN SessionParticipants sp ON sp.SessionId = s.Id
        LEFT JOIN Users u ON u.Id = sp.UserId

        WHERE s.Id = @Id
    ";

        var sessionDict = new Dictionary<int, StudySession>();

        await conn.QueryAsync<StudySession, Topic, SessionParticipant, User, StudySession>(
            sql,
            (session, topic, participant, user) =>
            {
                if (!sessionDict.TryGetValue(session.Id, out var existing))
                {
                    existing = session;

                    // attach topic
                    existing.Topic = topic;

                    existing.Participants = new List<SessionParticipant>();
                    sessionDict.Add(existing.Id, existing);
                }

                if (participant != null && participant.Id != 0 && user != null)
                {
                    participant.User = user;
                    existing.Participants!.Add(participant);
                }

                return existing;
            },
            new { Id = id },

            // IMPORTANT: order must match SELECT columns
            splitOn: "Id,Id,Id"
        );

        var final = sessionDict.Values.FirstOrDefault();

        if (final == null)
            return Result<StudySession>.Failure("Session not found");

        return Result<StudySession>.Success(final);
    }

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<Result<StudySession>> CreateAsync(StudySession session)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var sql = @"
            INSERT INTO StudySessions
            (Title, Description, TopicId, Location, StartTime, MaxParticipants, CurrentParticipants, CreatedByUserId)
            VALUES
            (@Title, @Description, @TopicId, @Location, @StartTime, @MaxParticipants, 0, @CreatedByUserId);

            SELECT CAST(SCOPE_IDENTITY() as int);
        ";

        var id = await conn.ExecuteScalarAsync<int>(sql, session);

        session.Id = id;
        session.CurrentParticipants = 0;

        return Result<StudySession>.Success(session); // FIXED
    }

    // -----------------------------
    // UPDATE
    // -----------------------------
    public async Task<Result> UpdateAsync(StudySession session)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var sql = @"
            UPDATE StudySessions
            SET Title = @Title,
                Description = @Description,
                TopicId = @TopicId,
                Location = @Location,
                StartTime = @StartTime,
                MaxParticipants = @MaxParticipants
            WHERE Id = @Id
        ";

        var rows = await conn.ExecuteAsync(sql, session);

        if (rows == 0)
            return Result.Failure("Session not found");

        return Result.Success();
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<Result> DeleteAsync(int id)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var sql = @"DELETE FROM StudySessions WHERE Id = @Id";

        var rows = await conn.ExecuteAsync(sql, new { Id = id });

        if (rows == 0)
            return Result.Failure("Session not found");

        return Result.Success();
    }

    // -----------------------------
    // JOIN SESSION
    // -----------------------------
    public async Task<Result> JoinSessionAsync(int sessionId, int userId)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            var alreadyJoined = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM SessionParticipants
                WHERE SessionId = @SessionId AND UserId = @UserId
            ", new { SessionId = sessionId, UserId = userId }, transaction);

            if (alreadyJoined > 0)
            {
                transaction.Rollback();
                return Result.Failure("User already joined");
            }

            // Sprint 3: reserve capacity with one atomic database update.
            // The WHERE clause is the concurrency guard: only requests that
            // still fit inside MaxParticipants can increment the counter.
            var reserveSeatSql = @"
                UPDATE StudySessions
                SET CurrentParticipants = CurrentParticipants + 1
                WHERE Id = @SessionId
                  AND CurrentParticipants < MaxParticipants
            ";

            var reservedSeats = await conn.ExecuteAsync(reserveSeatSql,
                new { SessionId = sessionId },
                transaction);

            if (reservedSeats == 0)
            {
                var sessionExists = await conn.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(1)
                    FROM StudySessions
                    WHERE Id = @SessionId
                ", new { SessionId = sessionId }, transaction);

                transaction.Rollback();

                return sessionExists == 0
                    ? Result.Failure("Session not found")
                    : Result.Failure("Session is full");
            }

            // UQ_SessionUser prevents duplicate joins, including racing
            // duplicate requests from the same user.
            var insertParticipantSql = @"
                INSERT INTO SessionParticipants (SessionId, UserId, JoinedAt)
                VALUES (@SessionId, @UserId, GETUTCDATE())
            ";

            await conn.ExecuteAsync(insertParticipantSql,
                new { SessionId = sessionId, UserId = userId },
                transaction);

            transaction.Commit();
            return Result.Success();
        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            transaction.Rollback();
            return Result.Failure("User already joined");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return Result.Failure(ex.Message);
        }
    }

    // -----------------------------
    // LEAVE SESSION
    // -----------------------------
    public async Task<Result> LeaveSessionAsync(int sessionId, int userId)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            var deleteSql = @"
                DELETE FROM SessionParticipants
                WHERE SessionId = @SessionId AND UserId = @UserId
            ";

            var rows = await conn.ExecuteAsync(deleteSql,
                new { SessionId = sessionId, UserId = userId },
                transaction);

            if (rows == 0)
            {
                transaction.Rollback();
                return Result.Failure("User is not part of session");
            }

            var updateSql = @"
                UPDATE StudySessions
                SET CurrentParticipants = CurrentParticipants - 1
                WHERE Id = @SessionId AND CurrentParticipants > 0
            ";

            await conn.ExecuteAsync(updateSql,
                new { SessionId = sessionId },
                transaction);

            transaction.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return Result.Failure(ex.Message);
        }
    }
}