using System.Data;
using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Solutions.Models;
using Dapper;
using FluentResults;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure;

public class SessionRepository : ISessionRepository
{
    private readonly IDbConnectionFactory _connection;
    private readonly ILogger<SessionRepository> _logger;
    private readonly IUserRepository _userRepository;

    public SessionRepository(IDbConnectionFactory connection, ILogger<SessionRepository> logger, IUserRepository userRepository)
    {
        _connection = connection;
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task DeleteExpiredSessions()
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    DELETE FROM session WHERE expirationtime_utc <= NOW();
                    """;
        await con.ExecuteAsync(query);
    }

    public async Task<bool> VerifyExerciseIdsAsync(List<int> exerciseIds, int authorId, IDbConnection con, IDbTransaction transaction)
    {
        var query = """
                    SELECT COUNT(*) FROM exercise WHERE exercise_id = ANY(@Ids) AND author_id = @AuthorId;
                    """;
        var result = await con.QuerySingleAsync<int>(query, new { Ids = exerciseIds.ToArray(), AuthorId = authorId } ,transaction);
        return exerciseIds.Count == result;
    }
    public async Task<int> InsertSessionAsync(Session session, int authorId)
    {
        using var con = await _connection.CreateConnectionAsync();
        using var transaction = con.BeginTransaction();
        try
        {

            var exercisesExist = await VerifyExerciseIdsAsync(session.Exercises, session.AuthorId, con, transaction);
            if (!exercisesExist)
            {
                _logger.LogWarning(
                    "Exercises did not exist for author {authorid}, tyring to create session {sessionTitle}", authorId,
                    session.Title);
                transaction.Rollback();
                return (int)SessionService.ErrorCodes.ExerciseDoesNotExist;
            }
            
            var languagesExists = await VerifyLanguagesIdsAsync(session.Languages);
            if (!languagesExists)
            {
                _logger.LogWarning("Languages {languages}, does not exist", session.Languages);
                transaction.Rollback();
                return (int)SessionService.ErrorCodes.LanguagesDoesNotExist;
            }

            var query = """
                        INSERT INTO session (title, description, author_id, expirationtime_utc, session_code) VALUES (@Title, @Description, @Author, @ExpirationTime, @SessionCode) RETURNING session_id;
                        """;
            var sessionId = await con.ExecuteScalarAsync<int>(query,
                new
                {
                    Title = session.Title, Description = session.Description,
                    Author = session.AuthorId,
                    ExpirationTime = session.ExpirationTimeUtc, SessionCode = session.SessionCode
                }, transaction);

            await InsertExerciseRelation(session.Exercises, sessionId, con, transaction);

            await InsertLanguageRelation(session.Languages.Select(x => (int)x).ToList(), sessionId, con, transaction);
            
            transaction.Commit();
            _logger.LogInformation("User: {userid} created Session: {sessionid}.", session.AuthorId, sessionId);
            return sessionId;
        }
        catch (PostgresException e)
        {
            transaction.Rollback();
            switch (e.SqlState)
            {
                case PostgresExceptions.UniqueConstraintViolation:
                    _logger.LogWarning("Session code not unique!");
                    return (int)SessionService.ErrorCodes.UniqueConstraintViolation;
                
                case PostgresExceptions.ForeignKeyViolation:
                    _logger.LogWarning("Exercises id's doesnt exist!");
                    return (int)SessionService.ErrorCodes.ExerciseDoesNotExist;
                default:
                    throw;
            }
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogWarning("Exception happend: {exception}, session not created!", e.Message);
            throw;
        }
    }

    public async Task InsertLanguageRelation(List<int> languageIds, int sessionId, IDbConnection con, IDbTransaction transaction)
    {
        var sessionLanguage = """
                                  INSERT INTO language_in_session(session_id, language_id) VALUES (@SessionId, @LanguageId);
                                  """;
        foreach (var langId in languageIds)
        {
            await con.ExecuteAsync(sessionLanguage, new { SessionId = sessionId, LanguageId = langId }, transaction);
        }
    }
    public async Task<Result> InsertExerciseRelation(List<int> exerciseIds, int sessionId, IDbConnection con, IDbTransaction transaction)
    {
        var exerciseQuery = """
                                INSERT INTO exercise_in_session (exercise_id, session_id) VALUES (@ExerciseId, @SessionId);
                                """;
        var AffectedRows =await con.ExecuteAsync(exerciseQuery,
            exerciseIds.Select(x => new { ExerciseId = x, SessionId = sessionId }).ToList(),
            transaction);

        if(AffectedRows != exerciseIds.Count())
        {
            _logger.LogError("Mismatch with inserted exercise-session relations. Relations inserted: {inserted}, exercises amount: {exercises}, session id: {sessionId}", AffectedRows, exerciseIds.Count(), sessionId);
            return Result.Fail("Inconsistency in inserted exercise/session relations");
        }

        return Result.Ok();
    }

    public async Task<bool> VerifyLanguagesIdsAsync(List<Language> languages)
    {
        var querry = """
                     SELECT COUNT(*) 
                     FROM language_support
                     WHERE language_id = ANY(ARRAY[@Languages]);
                     """;
        using var con = await _connection.CreateConnectionAsync();
        var result = await con.ExecuteScalarAsync<int>(querry, new { Languages = languages.Select(l => (int)l).ToList() });
        return languages.Count == result;
    }
    
    public async Task<int> CreateAnonUser(string name, int sessionId)
    {
        var result = await _userRepository.CreateAnonUserAsync(name, sessionId);
        if (result.IsFailed)
        {
            return 0;
        }
        return result.Value;
    }

    public async Task<bool> VerifyParticipantAccess(int userId, int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT COUNT(*) FROM anon_users
                    JOIN session 
                        ON session.session_id = anon_users.session_id
                    WHERE anon_users.user_id = @UserId AND session.session_id = @SessionId;
                    """;
        var result = await con.ExecuteScalarAsync<int>(query, new { UserId = userId, SessionId = sessionId });
        return result == 1;
    }

    public async Task<bool> VerifyAuthor(int userId, int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT COUNT(*) FROM app_users
                    JOIN session
                    ON session.author_id = app_users.user_id
                    WHERE app_users.user_id = @UserId AND session.session_id = @SessionId;
                    """;
        var result = await con.QuerySingleAsync<int>(query, new { UserId = userId, SessionId = sessionId });
        return result == 1;
    }

    public Task<int> CreateAnonUser(int sessionId)
    {
        throw new NotImplementedException();
    }

    public async Task<Session?> GetSessionOverviewAsync(int sessionId, int userId)
    {
        var query = """
                    SELECT session_id AS id, title, description, author_id AS authorid, expirationtime_utc AS ExpirationTimeUtc, app_users.name AS authorname  
                    FROM session
                    JOIN app_users ON app_users.user_id = session.author_id
                    WHERE session_id = @SessionId;
                    """;
        using var con = await _connection.CreateConnectionAsync();
        var session = await con.QueryFirstOrDefaultAsync<Session>(query, new { sessionId });
        if (session == null)
        {
            return null;
        }
        var exercisesQuery = """
                             SELECT e.exercise_id AS exerciseid, 
                                    title AS exercisetitle,
                                    CASE 
                                        WHEN s.user_id IS NOT NULL THEN true
                                        ELSE false
                                    END AS solved
                             FROM exercise AS e
                             JOIN exercise_in_session AS eis
                                ON e.exercise_id = eis.exercise_id
                             LEFT JOIN solved AS s 
                                ON e.exercise_id = s.exercise_id AND s.user_id = @UserId
                             WHERE eis.session_id = @SessionId;
                             """;
        var exercises = await con.QueryAsync<SolvedExercise>(exercisesQuery, new { SessionId = sessionId, UserId = userId });
        session.ExerciseDetails = exercises.ToList();
        
        return session;
    }
    
    public async Task<Result<Session>> GetSessionBySessionCodeAsync(string sessionCode)
    {
        var query = """
                    SELECT session_id AS id, title, description, author_id AS authorid, expirationtime_utc AS ExpirationTimeUtc, app_users.name AS authorname  
                    FROM session
                    JOIN app_users ON app_users.user_id = session.author_id
                    WHERE session_code = @SessionCode;
                    """;
        using var con = await _connection.CreateConnectionAsync();
        var session = await con.QueryFirstOrDefaultAsync<Session>(query, new { sessionCode });
        if (session == null)
        {
            return Result.Fail("Failed to find session"); 
        }
        
        var exercisesQuery = """
                             SELECT e.exercise_id AS exerciseid, title AS exercisetitle FROM exercise AS e
                             JOIN exercise_in_session AS eis
                             ON e.exercise_id = eis.exercise_id
                             WHERE eis.session_id = @SessionId;
                             """;
        var exercises = await con.QueryAsync<SolvedExercise>(exercisesQuery, new { SessionId = session.Id });
        session.ExerciseDetails = exercises.ToList();
            
        
        return Result.Ok(session);
    }
    
    public async Task<Session?> GetSessionByIdAsync(int sessionId)
    {
        var query = """
                    SELECT session_id AS id, title, description, author_id AS authorid, expirationtime_utc AS ExpirationTimeUtc, app_users.name AS authorname  
                    FROM session
                    JOIN app_users ON app_users.user_id = session.author_id
                    WHERE session_id = @SessionId;
                    """;
        using var con = await _connection.CreateConnectionAsync();
        var session = await con.QueryFirstOrDefaultAsync<Session>(query, new { sessionId });
        if (session == null)
        {
            return null;
        }
        var exercisesQuery = """
                             SELECT e.exercise_id AS exerciseid, title AS exercisetitle FROM exercise AS e
                             JOIN exercise_in_session AS eis
                             ON e.exercise_id = eis.exercise_id
                             WHERE eis.session_id = @SessionId;
                             """;
        var exercises = await con.QueryAsync<SolvedExercise>(exercisesQuery, new { sessionId });
        session.ExerciseDetails = exercises.ToList();
        
        return session;
    }

    public async Task<IEnumerable<Session>?> GetSessionsAsync(int authorId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT session_id AS id, title, expirationtime_utc AS ExpirationTimeUtc, session_code as sessioncode  
                    FROM session 
                    WHERE author_id = @Id
                    AND NOT EXISTS (
                        SELECT 1 
                        FROM session_in_classroom 
                        WHERE session_in_classroom.session_id = session.session_id
                    );
                    """;
        var results = await con.QueryAsync<Session>(query, new { Id = authorId });
        return results;
    }

    public async Task<bool> DeleteSessionAsync(int sessionId, int authorId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    DELETE FROM session WHERE session_id = @SessionId
                    AND EXISTS (
                        SELECT 1 FROM session WHERE session_id = @SessionId AND author_id = @AuthorId
                    )
                    """;
        var result = await con.ExecuteAsync(query, new { SessionId = sessionId, AuthorId = authorId });
        return result == 1;
    }
}