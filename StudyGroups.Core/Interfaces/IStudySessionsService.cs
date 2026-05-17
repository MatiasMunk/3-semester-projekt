using StudyGroups.Core.Models;

namespace StudyGroups.Core.Interfaces;

public interface IStudySessionService
{
    Task<Result<IEnumerable<StudySession>>> GetAllAsync(string? category);
    Task<Result<StudySession>> GetByIdAsync(int id);

    Task<Result<StudySession>> CreateAsync(StudySession session);
    Task<Result> UpdateAsync(StudySession session);
    Task<Result> DeleteAsync(int id);

    Task<Result> JoinSessionAsync(int sessionId, int userId);
    Task<Result> LeaveSessionAsync(int sessionId, int userId);
}