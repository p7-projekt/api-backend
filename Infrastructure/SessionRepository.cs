using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Dapper;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure;

public class SessionRepository : ISessionRepository
{
    private readonly IDbConnectionFactory _connection;
    private readonly ILogger<SessionRepository> _logger;

    public SessionRepository(IDbConnectionFactory connection, ILogger<SessionRepository> logger)
    {
        _connection = connection;
        _logger = logger;
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
        catch (PostgresException e) when (e.SqlState == "23505") // unqiue constraint violation
        {
            transaction.Rollback();
            _logger.LogWarning("Session code not unique!");
            return 0;
        }
        catch (PostgresException e) when (e.SqlState == "23503") // foreign key constraint violation
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

    // public async Task<bool> CheckSessionCodeIsValid(string sessionCode)
    // {
    //     using var con = await _connection.CreateConnectionAsync();
    //     var query = """
    //                 SELECT COUNT(*) FROM session WHERE session_code = @SessionCode;
    //                 """;
    //     var result = await con.ExecuteScalarAsync<int>(query, new { sessionCode });
    //     return result == 0;
    // }
}