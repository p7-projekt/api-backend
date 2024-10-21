using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Dapper;
using Infrastructure.Authentication.Contracts;
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
        _logger.LogInformation("Executed {query}", query);
    }

    public async Task<int> InsertSessionAsync(Session session)
    {
        using var con = await _connection.CreateConnectionAsync();
        using var transaction = con.BeginTransaction();
        try
        {

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

            // exercise relations
            var exerciseQuery = """
                                INSERT INTO exercise_in_session (exercise_id, session_id) VALUES (@ExerciseId, @SessionId);
                                """;
            await con.ExecuteAsync(exerciseQuery,
                session.Exercises.Select(x => new { ExerciseId = x, SessionId = sessionId }).ToList(),
                transaction);

            transaction.Commit();
            _logger.LogInformation("User: {userid} created Session: {sessionid}.", session.AuthorId, sessionId);
            return sessionId;
        }
        catch (PostgresException e) when (e.SqlState == PostgresExceptions.UniqueConstraintViolation.ToString())
        {
            transaction.Rollback();
            _logger.LogWarning("Session code not unique!");
            return 0;
        }
        catch (PostgresException e) when (e.SqlState == PostgresExceptions.ForeignKeyViolation.ToString())
        {
            transaction.Rollback();
            _logger.LogWarning("Exercises id's doesnt exist!");
            throw;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogWarning("Exception happend: {exception}, session not created!", e.Message);
            throw;
        }
    }

    public async Task<int> CreateAnonUser(int sessionId)
    {
        var result = await _userRepository.CreateAnonUserAsync(sessionId);
        if (result.IsFailed)
        {
            return 0;
        }
        return result.Value;
    }

    // public async Task<bool> VerifyParticipantAccess(int userId)
    // {
    //     using var con = await _connection.CreateConnectionAsync();
    //     var query = """
    //                 SELECT COUNT(*) 
    //                 FROM app_users
    //                 JOIN session
    //                 ON session.user_id = app_users.user_id
    //                 WHERE app_users.user_id = @UserId;
    //                 """;
    // }
    
    public async Task<Session?> GetSessionByIdAsync(int sessionId)
    {
        var query = """
                    SELECT session_id AS id, title, description, author_id AS authorid, expirationtime_utc AS ExpirationTimeUtc FROM session WHERE session_id = @SessionId;
                    """;
        using var con = await _connection.CreateConnectionAsync();
        var session = await con.QueryFirstOrDefaultAsync<Session>(query, new { sessionId });
        return session;
    }

    public async Task<bool> CheckSessionCodeIsValid(string sessionCode, int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT COUNT(*) FROM session WHERE session_code = @SessionCode AND session_id = @SessionId;
                    """;
        var result = await con.ExecuteScalarAsync<int>(query, new { SessionCode = sessionCode, SessionId = sessionId });
        _logger.LogInformation("Requesting check on session id {sessionid} with session code {sessioncode}", sessionId, sessionCode);
        return result == 1;
    }
}