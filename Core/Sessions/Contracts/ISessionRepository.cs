using Core.Sessions.Models;

namespace Core.Sessions.Contracts;

public interface ISessionRepository
{
    Task<int> InsertSessionAsync(Session session, int authorId);
    Task<bool> VerifyAuthor(int userId, int sessionId);
    Task<int> CreateAnonUser(int sessionId);
    Task<Session?> GetSessionByIdAsync(int sessionId);
    Task<bool> VerifyParticipantAccess(int userId, int sessionId);
    Task DeleteExpiredSessions();
    Task<Session?> GetSessionBySessionCodeAsync(string sessionCode);
    Task<Session?> GetSessionOverviewAsync(int sessionId, int userId);
    Task<IEnumerable<Session>?> GetSessionsAsync(int authorId);
    Task<bool> DeleteSessionAsync(int sessionId, int authorId);
}