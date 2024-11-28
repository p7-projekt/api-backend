using System.Data;
using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
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
        // https://www.postgresql.org/docs/current/queries-with.html#QUERIES-WITH-MODIFYING -- ALl statements work on the same snapshot
        // Expired timesession is executed first and have a data structure containg the ids of the sessions which where
        // deleted. Next we delete from users where the id exist within the user_in_timedsession, and check if the session id was deleted. 
        var query = """
                    WITH expired_timesessions AS (
                        DELETE FROM session
                        WHERE expirationtime_utc <= NOW()
                        AND expirationtime_utc IS NOT NULL
                        RETURNING session_id
                    ) 
                    DELETE FROM users
                    WHERE id IN (
                        SELECT user_id
                        FROM user_in_timedsession
                        WHERE session_id IN (SELECT session_id FROM expired_timesessions)
                    )
                    AND anonymous = true
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

    public async Task<Result> InsertLanguageRelation(List<int> languageIds, int sessionId, IDbConnection con, IDbTransaction transaction)
    {
        var sessionLanguage = """
                                  INSERT INTO language_in_session(session_id, language_id) VALUES (@SessionId, @LanguageId);
                                  """;
        var AffectedRows = await con.ExecuteAsync(sessionLanguage, 
                                                  languageIds.Select(x => new { SessionId = sessionId, LanguageId = x }).ToList(), 
                                                  transaction);
        if(AffectedRows != languageIds.Count())
        {
            _logger.LogError("Mismatch with inserted language-session relations. Relations inserted: {inserted}, language amount: {exercises}, session id: {sessionId}", AffectedRows, languageIds.Count(), sessionId);
            return Result.Fail("Inconsistency in inserted exercise/session relations");
        }
        return Result.Ok();
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
                    SELECT COUNT(*) FROM user_in_timedsession
                    where user_id = @UserId AND session_id = @SessionId
                    """;
        var result = await con.ExecuteScalarAsync<int>(query, new { UserId = userId, SessionId = sessionId });
        return result == 1;
    }

    public async Task<bool> VerifyAuthor(int userId, int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT COUNT(*) FROM users
                    JOIN session
                    ON session.author_id = users.id
                    WHERE users.id = @UserId AND session.session_id = @SessionId;
                    """;
        var result = await con.QuerySingleAsync<int>(query, new { UserId = userId, SessionId = sessionId });
        return result == 1;
    }

    public async Task<Session?> GetSessionOverviewAsync(int sessionId, int userId)
    {
        var query = """
                    SELECT session_id AS id, title, description, author_id AS authorid, expirationtime_utc AS ExpirationTimeUtc, users.name AS authorname  
                    FROM session
                    JOIN users ON users.id = session.author_id
                    WHERE session_id = @SessionId;
                    """;
        using var con = await _connection.CreateConnectionAsync();
        var getLanguages = """
                           SELECT ls.language_id AS id, ls.language
                           FROM language_in_session AS lis
                           JOIN language_support AS ls
                                ON lis.language_id = ls.language_id
                           WHERE session_id = @SessionId;
                           """;
        var session = await con.QueryFirstOrDefaultAsync<Session>(query, new { sessionId });
        if (session == null)
        {
            return null;
        }

        var languages = await con.QueryAsync<LanguageSupport>(getLanguages, new { SessionId = sessionId });
        session.LanguagesModel = languages.ToList();
        var exercisesQuery = """
                             SELECT e.exercise_id AS exerciseid,
                                    title AS exercisetitle,
                                    CASE
                                        WHEN s.solved IS NULL OR s.solved = false THEN false
                                        ELSE true
                                        END AS solved
                             FROM exercise AS e
                                      JOIN exercise_in_session AS eis
                                           ON e.exercise_id = eis.exercise_id
                                      LEFT JOIN submission AS s
                                ON e.exercise_id = s.exercise_id AND s.user_id = @UserId
                             WHERE eis.session_id = @SessionId;
                             """;
        var exercises = await con.QueryAsync<SolvedExercise>(exercisesQuery, new { SessionId = sessionId, UserId = userId });
        session.ExerciseDetails = exercises.ToList();
        
        return session;
    }

    public async Task<int> GetTimedSessionIdByUserId(int userId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT session_id
                    FROM user_in_timedsession
                    WHERE user_id = @UserId;
                    """;
        var result = await con.QueryFirstOrDefaultAsync<int>(query, new { UserId = userId });
        return result;
    }

    public async Task<Result> StudentJoinSession(string code, int userId)
    {
        using var con = await _connection.CreateConnectionAsync();
        using var transaction = con.BeginTransaction();
        try
        {

            var sessionExistQuery = """
                                    SELECT session_id
                                    FROM session
                                    WHERE session_code = @SessionCode
                                    """;
            var session = await con.QueryFirstOrDefaultAsync<int>(sessionExistQuery, new { SessionCode = code }, transaction);
            if (session == 0)
            {
                _logger.LogInformation("Sessioncode {sessionCode} was wrong!", code);
                return Result.Fail("Invalid session code");
            }

            var alreadyJoinedQuery = """
                                     SELECT COUNT(*) 
                                     FROM user_in_timedsession
                                     WHERE user_id = @UserId
                                     AND session_id = @SessionId
                                     """;
            var joinedResult = con.ExecuteScalar<int>(alreadyJoinedQuery, new { UserId = userId, SessionId = session }, transaction);
            if (joinedResult != 0)
            {
                return Result.Fail("Already joined");
            }
            
            var insertRelationQuery = """
                                      INSERT INTO user_in_timedsession
                                      (user_id, session_id)
                                      VALUES
                                      (@UserId, @SessionId);
                                      """;
            await con.ExecuteScalarAsync<int>(insertRelationQuery, new { UserId = userId, SessionId = session }, transaction);
            _logger.LogInformation("User with id {userId} is added to session id {sessionId}", userId, session);
            transaction.Commit();
        }
        catch (Exception e)
        {
            _logger.LogWarning("Exception happend: {exception}, userd did not join session!", e.Message);
            transaction.Rollback();
        }
        return Result.Ok();
    }

    
    public async Task<Result<Session>> GetSessionBySessionCodeAsync(string sessionCode)
    {
        var query = """
                    SELECT session_id AS id, title, description, author_id AS authorid, expirationtime_utc AS ExpirationTimeUtc, users.name AS authorname  
                    FROM session
                    JOIN users ON users.id = session.author_id
                    WHERE session_code = @SessionCode;
                    """;
        using var con = await _connection.CreateConnectionAsync();
        var session = await con.QueryFirstOrDefaultAsync<Session>(query, new { sessionCode });
        if (session == null)
        {
            return Result.Fail("Failed to find session"); 
        }
       
        session.ExerciseDetails = await GetExercisesOfSessionAsync(session.Id, con);
            
        return Result.Ok(session);
    }
    
    public async Task<Session?> GetSessionByIdAsync(int sessionId)
    {
        var query = """
                    SELECT session_id AS id, title, description, author_id AS authorid, expirationtime_utc AS ExpirationTimeUtc, users.name AS authorname  
                    FROM session
                    JOIN users ON users.id = session.author_id
                    WHERE session_id = @SessionId;
                    """;
        using var con = await _connection.CreateConnectionAsync();
        var session = await con.QueryFirstOrDefaultAsync<Session>(query, new { sessionId });
        if (session == null)
        {
            return null;
        }
        session.ExerciseDetails = await GetExercisesOfSessionAsync(sessionId, con);
        
        return session;
    }

    public async Task<List<SolvedExercise>>GetExercisesOfSessionAsync(int sessionId, IDbConnection con)
    {
        var exercisesQuery = """
                             SELECT e.exercise_id AS exerciseid, title AS exercisetitle FROM exercise AS e
                             JOIN exercise_in_session AS eis
                             ON e.exercise_id = eis.exercise_id
                             WHERE eis.session_id = @SessionId;
                             """;
        var exercises = await con.QueryAsync<SolvedExercise>(exercisesQuery, new { sessionId });
        return exercises.OrderBy(x => x.ExerciseId).ToList();
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
        // owned session is not a data modyfing query, which means it will only be executed once it is referenced.
        // where as the delete_anon_users is a data modifying query and will be executed exactly once 
        // delete_anon_users is thus triggered just before the last delete of the session.
        var query = """
                    WITH owned_session AS (
                        SELECT 1
                        FROM session
                        WHERE session_id = @SessionId
                          AND author_id = @AuthorId
                    ), delete_anon_users AS (
                        DELETE FROM users
                        WHERE id IN (
                            SELECT user_id
                            FROM user_in_timedsession
                            WHERE session_id = @SessionId
                        ) AND anonymous = true
                        AND EXISTS (SELECT 1 FROM owned_session)
                        RETURNING id
                        )
                    DELETE FROM session
                    WHERE session_id = @SessionId
                    AND EXISTS (SELECT 1 FROM owned_session);
                    """;
        var result = await con.ExecuteAsync(query, new { SessionId = sessionId, AuthorId = authorId });
        return result == 1;
    }
}