using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Dapper;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;

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
            foreach (var exercise in session.Exercises)
            {
                await con.ExecuteAsync(exerciseQuery, new { ExerciseId = exercise, SessionId = sessionId }, transaction);
            }
            
            transaction.Commit();
            _logger.LogInformation("User: {userid} created Session: {sessionid}.", session.AuthorId, sessionId);
            return sessionId;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogWarning("Exception happend: {exception}, session not created!", e.Message);
        }

        return 0;
    }
}