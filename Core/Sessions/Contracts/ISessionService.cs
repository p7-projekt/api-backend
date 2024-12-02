using Core.Sessions.Models;
using Core.Shared;
using FluentResults;

namespace Core.Sessions.Contracts;

public interface ISessionService
{
    Task<Result<CreateSessionResponseDto>> CreateSessionAsync(CreateSessionDto sessionDto, int authorId);
    Task<Result<JoinResponseDto>> JoinSessionAnonUser(JoinDto dto);
    Task<Result<GetSessionResponseDto>> GetSessionByIdAsync(int sessionId, int userId, Roles role);
    Task<Result<List<GetSessionsResponseDto>>> GetSessions(int userId, Roles role);
    Task<Result<JoinResponseDto>> JoinStudent(int userId, string code);
    Task<Result> DeleteSession(int sessionId, int userId);
}