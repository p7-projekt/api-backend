using Core.Exercises.Contracts;
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

    public SessionService(ISessionRepository sessionRepository, ILogger<SessionService> logger, IAnonTokenService tokenService)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
        _tokenService = tokenService;
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
        
        var sessionId = await _sessionRepository.InsertSessionAsync(session, authorId);
        if (sessionId == (int)ErrorCodes.UniqueConstraintViolation)
        {
            // sessionCode not unique
            _logger.LogInformation("Session code {sessionCode} is already in use!", sessionCode);
            sessionCode = GenerateSessionCode();
            session.SessionCode = sessionCode;
            var limit = 10;
            while (await _sessionRepository.InsertSessionAsync(session, authorId) == (int)ErrorCodes.UniqueConstraintViolation && limit > 0)
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
            _logger.LogInformation("Succesfully created a unique session code: {sessionCode}", sessionCode);
        } else if (sessionId == (int)ErrorCodes.ExerciseDoesNotExist)
        {
            return Result.Fail("Exercises for Author could not be found");
        }

        if (sessionId == (int)ErrorCodes.LanguagesDoesNotExist)
        {
            return Result.Fail("Languages could not be found");
        }

        return new CreateSessionResponseDto(sessionId, sessionCode);
    }

    public async Task<Result<JoinSessionResponseDto>> JoinStudent(int userId, string code)
    {
        Codes actualCode = DetermineCode(code);
        if (actualCode == Codes.None)
        {
            return Result.Fail("Invalid code");
        }

        if (actualCode == Codes.SessionCode)
        {
            return await JoinTimedSessionStudent(userId, code);
        }

        if (actualCode == Codes.ClassRoomCode)
        {
            throw new NotImplementedException();
        }
        
        return Result.Fail("Internal error happend");
    }

    private async Task<Result<JoinSessionResponseDto>> JoinTimedSessionStudent(int userId, string code)
    {
        var result = await _sessionRepository.StudentJoinSession(code, userId);
        if (result.IsFailed)
        {
            return result;
        }
        return new JoinSessionResponseDto(null, null);
    }

    // private async Task<Result> JoinClassRoomStudent()
    // {
    //     TODO
    // }
    
    public enum Codes
    {
        None,
        SessionCode,
        ClassRoomCode,
    }

    private Codes DetermineCode(string code)
    {
        if (char.IsUpper(code[0]) && char.IsUpper(code[1]))
        {
            _logger.LogInformation("Code {code} determined to be {codes}", code, nameof(Codes.SessionCode));
            return Codes.SessionCode;
        }

        if (char.IsUpper(code[4]) && char.IsUpper(code[5]))
        {
            _logger.LogInformation("Code {code} determined to be {codes}", code, nameof(Codes.ClassRoomCode));
            return Codes.ClassRoomCode;
        }
        _logger.LogInformation("Code {code} determined to be {codes}", code, nameof(Codes.None));
        return Codes.None;
    }

    public async Task<Result<JoinSessionResponseDto>> JoinSessionAnonUser(JoinSessionDto dto)
    {
        if (dto.Name == null)
        {
            return Result.Fail("Missing name");
        }
        // create anon user entry in table
        var session = await _sessionRepository.GetSessionBySessionCodeAsync(dto.SessionCode);
        if (session.IsFailed)
        {
            return Result.Fail("Invalid session");
        }
        var student = await _sessionRepository.CreateAnonUser(dto.Name, session.Value.Id);
        
        var timeOffset = session.Value.ExpirationTimeUtc - DateTime.UtcNow;
        
        var createToken = _tokenService.GenerateAnonymousUserJwt((int)Math.Ceiling(timeOffset.TotalMinutes), student);
        return new JoinSessionResponseDto(createToken, DateTime.UtcNow.AddMinutes((int)Math.Ceiling(timeOffset.TotalMinutes)));
    }

    public async Task<Result<GetSessionResponseDto>> GetSessionByIdAsync(int sessionId, int userId, Roles role)
    {
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
        
        
        var session = await _sessionRepository.GetSessionOverviewAsync(sessionId, userId);
        if (session == null)
        {
            _logger.LogInformation("Session {sessionid}, request by {userid} does not exist", sessionId, userId);
            return Result.Fail("Session does not exist");
        }

        
        return session.ConvertToGetResponse();
    }
    public async Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInSessionAsync(int sessionId, int userId)
    {
        var access = false;
        access = await _sessionRepository.VerifyAuthor(userId, sessionId);
        if (!access)
        {
            _logger.LogInformation("User {userid} does not have access to {sessionid}", userId, sessionId);
            return Result.Fail("User does not have access to session");
        }
        var usersConnected = await _sessionRepository.GetConnectedUsersAsync(sessionId);

        var exercises = await _sessionRepository.GetExercisesInSessionAsync(sessionId);
        if (exercises == null || exercises.Count() == 0)
        {
            _logger.LogInformation("No Exercises in session: {sessionID}", sessionId);
            return Result.Fail("Exercises not found");
        }
        var combinedDtos = exercises.Select(dto => new GetExercisesAndUserDetailsInSessionResponseDto(
        dto.Id,
        dto.Solved,
        dto.Attempted,
        dto.UserIds.Zip(dto.Names, (id, name) => new UserDetailDto(id, name)).ToList())).ToList();

        var combinedInfoDto = new GetExercisesInSessionCombinedInfo(usersConnected, combinedDtos);

        return Result.Ok(combinedInfoDto);
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
        UniqueConstraintViolation = 0,
        ExerciseDoesNotExist = -1,
        LanguagesDoesNotExist = -2
    }
}