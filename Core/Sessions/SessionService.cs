using Core.Sessions.Contracts;
using Core.Sessions.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Sessions;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ISessionRepository sessionRepository, ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<Result<CreateSessionResponseDto>> CreateSessionAsync(CreateSessionDto sessionDto, int authorId)
    {
        var sessionCode = GenerateSessionCode();
        var session = sessionDto.ConvertToSession();
        session.AuthorId = authorId;
        session.SessionCode = sessionCode;

        var sessionId = await _sessionRepository.InsertSessionAsync(session);
        if (sessionId == 0)
        {
            // sessionCode not unique
            _logger.LogInformation("Session code {sessionCode} is already in use!", sessionCode);
            sessionCode = GenerateSessionCode();
            session.SessionCode = sessionCode;
            var limit = 10;
            while (await _sessionRepository.InsertSessionAsync(session) == 0 && limit > 0)
            {
                sessionCode = GenerateSessionCode();
                session.SessionCode = sessionCode;
                limit--;
            }

            if (limit == 0)
            {
                _logger.LogInformation("Failed to create session for {userid}", authorId);
                return Result.Fail("Error creating session");
            }
            _logger.LogInformation("Succesfully created a unique a unqiue session code: {sessionCode}", sessionCode);
        }

        return new CreateSessionResponseDto(sessionId, sessionCode);
    }

    public string GenerateSessionCode()
    {
        Random rnd = new Random();
        // Create a random uppercase Char
        //65-90 inclusive
        var firstChar = (char)rnd.Next(65, 91);
        var secondChar = (char)rnd.Next(65, 91);
        var pinCode = rnd.Next(1000, 10000);
        var chars = string.Concat(firstChar, secondChar);
        return string.Concat(chars, pinCode.ToString());
    }
}