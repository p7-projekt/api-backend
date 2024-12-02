using Core.Classrooms.Contracts;
using Core.Exercises.Contracts;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using Core.Shared.Contracts;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;

namespace Core.Sessions;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionService> _logger;
    private readonly IAnonTokenService _tokenService;
    private readonly IClassroomRepository _classroomRepository;

    public SessionService(ISessionRepository sessionRepository, ILogger<SessionService> logger, IAnonTokenService tokenService, IClassroomRepository classroomRepository)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
        _tokenService = tokenService;
        _classroomRepository = classroomRepository;
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
    
    public async Task<Result<List<GetSessionsResponseDto>>> GetSessions(int userId, Roles role)
    {
        var sessions = role switch
        {
            Roles.Instructor => await _sessionRepository.GetInstructorSessionsAsync(userId),
            Roles.Student => await _sessionRepository.GetStudentSessionsAsync(userId),
            _ => null
        };
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

    public async Task<Result<JoinResponseDto>> JoinStudent(int userId, string code)
    {
        Codes actualCode = DetermineCode(code);
        return actualCode switch
        {
            Codes.None => Result.Fail("Invalid code"),
            Codes.SessionCode => await JoinTimedSessionStudent(userId, code),
            Codes.ClassRoomCode => await JoinClassRoomStudent(userId, code),
            _ => Result.Fail("Internal error happend")
        };
    }

    private async Task<Result<JoinResponseDto>> JoinTimedSessionStudent(int userId, string code)
    {
        var result = await _sessionRepository.StudentJoinSession(code, userId);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }
        return new JoinResponseDto(null, null, JoinedType.TimedSession, result.Value);
    }

    private async Task<Result<JoinResponseDto>> JoinClassRoomStudent(int studentId, string code)
    {
        var result = await _classroomRepository.JoinClassroomAsync(studentId, code);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }
        return new JoinResponseDto(null, null, JoinedType.Classroom, result.Value);
    }

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

    public async Task<Result<JoinResponseDto>> JoinSessionAnonUser(JoinDto dto)
    {
        if (dto.Name == null)
        {
            return Result.Fail("Missing name");
        }
        // create anon user entry in table
        var session = await _sessionRepository.GetSessionBySessionCodeAsync(dto.Code);
        if (session.IsFailed)
        {
            return Result.Fail("Invalid session");
        }
        var student = await _sessionRepository.CreateAnonUser(dto.Name, session.Value.Id);
        
        var timeOffset = session.Value.ExpirationTimeUtc - DateTime.UtcNow;
        
        var createToken = _tokenService.GenerateAnonymousUserJwt((int)Math.Ceiling(timeOffset.TotalMinutes), student);
        return new JoinResponseDto(createToken, DateTime.UtcNow.AddMinutes((int)Math.Ceiling(timeOffset.TotalMinutes)), JoinedType.TimedSession, null);
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

        if (role == Roles.Instructor)
        {
            session.ExerciseDetails.ForEach(e => e.Solved = null);
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
        UniqueConstraintViolation = 0,
        ExerciseDoesNotExist = -1,
        LanguagesDoesNotExist = -2
    }
}