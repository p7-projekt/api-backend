using Core.Exercises.Models;
using Core.Sessions.Models;
using Core.Shared;
using FluentResults;

namespace Core.Sessions.Contracts;

public interface ISessionService
{
    Task<Result<CreateSessionResponseDto>> CreateSessionAsync(CreateSessionDto sessionDto, int authorId);
    Task<Result<JoinSessionResponseDto>> JoinSessionAnonUser(JoinSessionDto dto);
    Task<Result<GetSessionResponseDto>> GetSessionByIdAsync(int sessionId, int userId, Roles role);
    Task<Result<List<GetSessionsResponseDto>>> GetSessions(int userId);
    Task<Result> DeleteSession(int sessionId, int userId);
    Task<Result<List<GetExercisesInSessionResponseDto>>> GetExercisesInSessionAsync(int sessionId);

}