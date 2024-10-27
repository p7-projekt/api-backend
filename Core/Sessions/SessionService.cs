using Core.Exercises.Contracts.Repositories;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using Core.Shared.Contracts;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Sessions;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionService> _logger;
    private readonly IAnonTokenService _tokenService;
    private readonly IExerciseRepository _exerciseRepository;

    public SessionService(ISessionRepository sessionRepository, ILogger<SessionService> logger, IAnonTokenService tokenService, IExerciseRepository exerciseRepository)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
        _tokenService = tokenService;
        _exerciseRepository = exerciseRepository;
    }

    public async Task<Result> DeleteSession(int sessionId, int userId)
    {
        var result = await _sessionRepository.DeleteSessionAsync(sessionId, userId);
        if (!result)
        {
            return Result.Fail("Session could not be deleted");
        }
        return Result.Ok();
    }
    
    public async Task<Result<List<GetSessionsResponseDto>>> GetSessions(int userId)
    {
        var sessions = await _sessionRepository.GetSessionsAsync(userId);
        if (sessions == null)
        {
            return Result.Fail("Sessions not found");
        }

        return Result.Ok(sessions.Select(x => x.ConvertToGetSessionsResponse()).ToList());
    }
    
    public async Task<Result<CreateSessionResponseDto>> CreateSessionAsync(CreateSessionDto sessionDto, int authorId)
    {
        var sessionCode = GenerateSessionCode();
        var session = sessionDto.ConvertToSession();
        session.AuthorId = authorId;
        session.SessionCode = sessionCode;
        
        // validate if exercises exists..
        var exercisesExist = await _exerciseRepository.VerifyExerciseIdsAsync(session.Exercises, authorId);
        if (!exercisesExist)
        {
            return Result.Fail("Exercises not found");
        }
        
        var sessionId = await _sessionRepository.InsertSessionAsync(session);
        if (sessionId == (int)ErrorCodes.UniqueConstraintViolation)
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

    public async Task<Result<JoinSessionResponseDto>> JoinSessionAnonUser(JoinSessionDto dto, int sessionId)
    {
        // check token exists
        var isTokenAndSessionValid = await _sessionRepository.CheckSessionCodeIsValid(dto.SessionCode, sessionId);
        if (!isTokenAndSessionValid)
        {
            return Result.Fail($"{nameof(dto.SessionCode)} is invalid!");
        }
        
        // create anon user entry in table
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            return Result.Fail("Invalid session");
        }
        var student = await _sessionRepository.CreateAnonUser(sessionId);
        
        var timeOffset = session.ExpirationTimeUtc - DateTime.UtcNow;
        
        var createToken = _tokenService.GenerateAnonymousUserJwt((int)Math.Ceiling(timeOffset.TotalMinutes), student);
        return new JoinSessionResponseDto(createToken);
    }

    public async Task<Result<GetSessionResponseDto>> GetSessionByIdAsync(int sessionId, int userId, Roles role)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogInformation("Session {sessionid}, request by {userid} does not exist", sessionId, userId);
            return Result.Fail("Session does not exist");
        }

        var access = false;
        if (role == Roles.Instructor)
        {
            access = await _sessionRepository.VerifyAuthor(userId, sessionId);
        }
        else
        {
            access = await _sessionRepository.VerifyParticipantAccess(userId, sessionId);
        }
        if (!access)
        {
            _logger.LogInformation("User {userid} does not have access to {sessionid}", userId, sessionId);
            return Result.Fail("User does not have access to session");
        }
        
        return session.ConvertToGetResponse();
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
    
    public enum ErrorCodes
    {
        UniqueConstraintViolation = 0
    }
}