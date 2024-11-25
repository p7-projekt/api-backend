using Core.Dashboards.Models;
using Core.Dashboards.Contracts;
using Dapper;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _connection;
    private readonly ILogger<ClassroomRepository> _logger;
    public DashboardRepository(ILogger<ClassroomRepository> logger, IDbConnectionFactory connection)
    {
        _logger = logger;
        _connection = connection;
    }

    public async Task<IEnumerable<GetExercisesInSessionResponseDto>?> GetExercisesInTimedSessionAsync(int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT
                    sub.exercise_id AS Id,
                    COUNT(CASE WHEN solved THEN 1 END)::int AS Solved,
                    COUNT(exercise_id)::int AS Attempted,
                    ARRAY_REMOVE(ARRAY_AGG(CASE WHEN solved THEN u.user_id END), NULL) AS UserIds,
                    ARRAY_REMOVE(ARRAY_AGG(CASE WHEN solved THEN _user.name END), NULL):: text[] AS Names
                FROM
                    session AS s
                    JOIN user_in_timedsession AS u ON s.session_id = u.session_id
                    JOIN submission AS sub ON u.user_id = sub.user_id AND s.session_id = sub.session_id
                    JOIN users AS _user ON u.user_id = _user.id
                WHERE
                    s.session_id = @Id
                GROUP BY
                    sub.exercise_id;
                """;
        var results = await con.QueryAsync<dynamic>(query, new { Id = sessionId });
        var mappedResults = results.Select(row => new GetExercisesInSessionResponseDto(
            (int)row.id,
            (int)row.solved,
            (int)row.attempted,
            ((IEnumerable<int>)row.userids).ToArray(),
            ((IEnumerable<string>)row.names).ToArray()
            )).ToList();

        return mappedResults;
    }

    public async Task<IEnumerable<GetExercisesInSessionResponseDto>?> GetExercisesInClassSessionAsync(int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
                    SELECT
                    sub.exercise_id AS Id,
                    COUNT(CASE WHEN solved THEN 1 END)::int AS Solved,
                    COUNT(exercise_id)::int AS Attempted,
                    ARRAY_REMOVE(ARRAY_AGG(CASE WHEN solved THEN u.user_id END), NULL) AS UserIds,
                    ARRAY_REMOVE(ARRAY_AGG(CASE WHEN solved THEN _user.name END), NULL):: text[] AS Names
                FROM
                    session AS s
                    JOIN session_in_classroom AS sc ON s.session_id = sc.session_id
                    JOIN student_in_classroom AS student ON sc.classroom_id = student.classroom_id
                    JOIN submission AS sub ON student.student_id = sub.user_id AND s.session_id = sub.session_id
                    JOIN users AS _user ON student.student_id = _user.id
                WHERE
                    s.session_id = @Id
                GROUP BY
                    sub.exercise_id;
                """;
        var results = await con.QueryAsync<dynamic>(query, new { Id = sessionId });
        var mappedResults = results.Select(row => new GetExercisesInSessionResponseDto(
            (int)row.id,
            (int)row.solved,
            (int)row.attempted,
            ((IEnumerable<int>)row.userids).ToArray(),
            ((IEnumerable<string>)row.names).ToArray()
            )).ToList();

        return mappedResults;
    }

    public async Task<int> GetConnectedTimedUsersAsync(int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
            SELECT COUNT(*)
            FROM Session AS s
                JOIN user_in_timedsession AS u ON s.session_id = u.session_id
            WHERE s.session_id = @Id;
            """;
        var results = await con.QueryFirstOrDefaultAsync<int>(query, new { Id = sessionId });
        return results;
    }
    public async Task<int> GetConnectedUsersClassAsync(int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var query = """
            SELECT COUNT(*)
            FROM Session AS s
                JOIN session_in_classroom AS sic ON s.session_id = sic.session_id
                JOIN student_in_classroom AS students ON sic.classroom_id = students.classroom_id
            WHERE s.session_id = @Id;
            """;
        var results = await con.QueryFirstOrDefaultAsync<int>(query, new { Id = sessionId });
        return results;
    }
}
