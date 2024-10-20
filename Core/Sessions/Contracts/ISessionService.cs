using Core.Sessions.Models;
using FluentResults;

namespace Core.Sessions.Contracts;

public interface ISessionService
{
    Task<Result<CreateSessionResponseDto>> CreateSessionAsync(CreateSessionDto sessionDto, int authorId);
    Task<Result<string>> JoinSessionAnonUser(JoinSessionDto dto, int sessionId);
}