using Core.Sessions.Models;

namespace Core.Sessions.Contracts;

public interface ISessionRepository
{
    Task<int> InsertSessionAsync(Session session);
    Task<bool> CheckSessionCodeIsValid(string sessionCode, int sessionId);
    Task<int> CreateAnonUser(int sessionId);
    Task<Session?> GetSessionByIdAsync(int sessionId);
}