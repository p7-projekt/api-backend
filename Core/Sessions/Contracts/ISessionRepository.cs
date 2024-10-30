using Core.Sessions.Models;

namespace Core.Sessions.Contracts;

public interface ISessionRepository
{
    Task<int> InsertSessionAsync(Session session, int authorId);
    // Task<bool> CheckSessionCodeIsValid(string sessionCode, int sessionId);
    Task<Session?> GetSessionBySessionCodeAsync(string sessionCode);
    Task<bool> VerifyAuthor(int userId, int sessionId);
    Task<int> CreateAnonUser(int sessionId);
    Task<Session?> GetSessionByIdAsync(int sessionId);
    Task<bool> VerifyParticipantAccess(int userId, int sessionId);
    Task DeleteExpiredSessions();

    Task<IEnumerable<Session>?> GetSessionsAsync(int authorId);
    
    Task<bool> DeleteSessionAsync(int sessionId, int authorId);
}