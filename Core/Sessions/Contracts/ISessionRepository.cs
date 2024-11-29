using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Sessions.Models;
using FluentResults;
using System.Data;

namespace Core.Sessions.Contracts;

public interface ISessionRepository
{
    Task<int> InsertSessionAsync(Session session, int authorId);
    Task<bool> VerifyAuthor(int userId, int sessionId);
    Task<int> CreateAnonUser(string name, int sessionId);
    Task<Session?> GetSessionByIdAsync(int sessionId);
    Task<List<SolvedExercise>> GetExercisesOfSessionAsync(int sessionId, IDbConnection con);
    Task<bool> VerifyParticipantAccess(int userId, int sessionId);
    Task DeleteExpiredSessions();
    Task<Result<Session>> GetSessionBySessionCodeAsync(string sessionCode);
    Task<Session?> GetSessionOverviewAsync(int sessionId, int userId);
    Task<List<Session>?> GetInstructorSessionsAsync(int authorId);
    Task<List<Session>?> GetStudentSessionsAsync(int studentId);
    Task<bool> DeleteSessionAsync(int sessionId, int authorId);
    Task<bool> VerifyExerciseIdsAsync(List<int> exerciseIds, int authorId, IDbConnection con, IDbTransaction transaction);
    Task<bool> VerifyLanguagesIdsAsync(List<Language> languages);
    Task<Result> InsertExerciseRelation(List<int> exerciseIds, int sessionId, IDbConnection con, IDbTransaction transaction);
    Task<Result> InsertLanguageRelation(List<int> languageIds, int sessionId, IDbConnection con, IDbTransaction transaction);
    Task<Result<int>> StudentJoinSession(string code, int userId);
    Task<int> GetTimedSessionIdByUserId(int userId);
}