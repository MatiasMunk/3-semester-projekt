using StudyGroups.Contracts;

namespace StudyGroups.Http.Interfaces;

/*
 * API abstraction for all studysession-related operations.
 * This is used by the WinForms client to communicate with the backend.
 */

public interface IStudySessionApi
{
    Task<IEnumerable<StudySessionDto>?> GetAll(string? category);
    Task<StudySessionDto?> GetById(int sessionId);
    Task Create(CreateStudySessionRequest request);
    Task Join(int sessionId, int userId);
    Task Leave(int sessionId, int userId);
}