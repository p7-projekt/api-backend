using Core.Classrooms.Contracts;
using Core.Classrooms.Models;
using Core.Languages.Models;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Dapper;
using FluentResults;
using Infrastructure.Persistence.Contracts;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Quartz.Simpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public class ClassroomRepository : IClassroomRepository
{
    private readonly IDbConnectionFactory _connection;
    private readonly ILogger<ClassroomRepository> _logger;
    private readonly ISessionRepository _sessionRepository;
    public ClassroomRepository(ILogger<ClassroomRepository> logger, IDbConnectionFactory connection, ISessionRepository sessionRepository)
    {
        _logger = logger;
        _connection = connection;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result> InsertClassroomAsync(ClassroomDto dto, int authorId, string roomcode)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "INSERT INTO classroom (title, description, owner, roomcode, registration_open) VALUES (@Title, @Description, @AuthorId, @Roomcode, FALSE) RETURNING classroom_id;";

        var result = await con.ExecuteAsync(query, new { Title = dto.Title, Description = dto.Description, AuthorId = authorId, RoomCode = roomcode } );

        if (result == 0)
        {
            _logger.LogWarning("Failed to insert new classroom of user {author_id}", authorId);
            return Result.Fail("Failed to insert new classroom");
        }

        return Result.Ok();
    }

    public async Task<Result> AddSessionToClassroomAsync(ClassroomSessionDto dto, int authorId, int classroomId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var transaction = con.BeginTransaction();
        try
        {
            var correctAuthor = await VerifyClassroomAuthor(classroomId, authorId);
            if (!correctAuthor)
            {
                return Result.Fail("Failed to validate author of classroom");
            }

            var verifiedExercises = await _sessionRepository.VerifyExerciseIdsAsync(dto.ExerciseIds, authorId, con, transaction);
            if (!verifiedExercises)
            {
                transaction.Rollback();
                return Result.Fail("Exercises not associated with instructor");
            }

            var verifiedLanguages = await _sessionRepository.VerifyLanguagesIdsAsync(dto.LanguageIds.Select(l => (Language)l).ToList());
            if (!verifiedLanguages)
            {
                transaction.Rollback();
                return Result.Fail("Languages not valid");
            }

            var createSessionQuery = "INSERT INTO session (title, description, author_id) VALUES(@Title, @Description, @AuthorId) RETURNING session_id;";
            var sessionId = await con.QuerySingleAsync<int>(createSessionQuery, new { Title = dto.Title, Description = dto.Description, AuthorId = authorId }, transaction);

            var addSessionQuery = "INSERT INTO session_in_classroom (classroom_id, session_id, active) VALUES (@ClassroomId, @SessionId, FALSE);";
            await con.ExecuteAsync(addSessionQuery, new { ClassroomId = classroomId, SessionId = sessionId }, transaction);

            await _sessionRepository.InsertExerciseRelation(dto.ExerciseIds, sessionId, con, transaction);
            await _sessionRepository.InsertLanguageRelation(dto.LanguageIds, sessionId, con, transaction);

        } catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError("Error occured during creation of classroom sesssion: {exception}", ex.Message);
            throw;
        }
        transaction.Commit();

        return Result.Ok();
    }

    public async Task<Result> DeleteClassroomAsync(int classroomId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var transaction = con.BeginTransaction();
        try
        {
            var getSessionsQuery = "SELECT session_id FROM session_in_classroom WHERE classroom_id = @ClassroomId;";
            var sessionIds = await con.QueryAsync<int>(getSessionsQuery, new { ClassroomId = classroomId });

            var deleteSessionsQuery = "DELETE FROM session WHERE session_id = ANY(@SessionIds);";
            var removed = await con.ExecuteAsync(deleteSessionsQuery, new { SessionIds = sessionIds.ToArray() }, transaction);
            if (removed != sessionIds.Count())
            {
                transaction.Rollback();
                _logger.LogWarning("Mismatch between deleted sessions, and sessions associated to classroom of id {classroomId}", classroomId);
                return Result.Fail("Failed to delete sessions of classroom");
            }

            var deleteClassroomQuery = "DELETE FROM classroom WHERE classroom_id = @ClassroomId";
            await con.ExecuteAsync(deleteClassroomQuery, new { ClassroomId = classroomId }, transaction);
        } catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError("Error occured during deletion of classroom with id {classroom_id}: {exception}", classroomId, ex.Message);
            return Result.Fail("Error occurred during deletion of classroom");
        }
        transaction.Commit();

        return Result.Ok();
    }

    public async Task<Result<GetClassroomResponseDto>> GetClassroomByIdAsync(int classroomId)
    {
        try
        {
            using var con = await _connection.CreateConnectionAsync();

            var classroomQuery = """
                                 SELECT classroom_id AS id, title, description, roomcode, registration_open AS isOpen,
                                   (SELECT COUNT(*) 
                                   FROM student_in_classroom 
                                   WHERE classroom_id = @ClassroomId) AS totalstudents
                                 FROM classroom 
                                 WHERE classroom_id = @ClassroomId;
                                 """;
            var classroom = await con.QuerySingleAsync<GetClassroomResponseDto>(classroomQuery, new { ClassroomId = classroomId });

            var sessionIdsQuery = """
                              SELECT s.session_id AS id, s.title, sic.active
                              FROM session_in_classroom AS sic
                              JOIN session AS s 
                              ON s.session_id = sic.session_id
                              WHERE classroom_id = @ClassroomId
                              """;

            var sessions = await con.QueryAsync<GetClassroomSessionDto>(sessionIdsQuery, new { ClassroomId = classroomId });
            classroom.Sessions = sessions.ToList();

            return classroom;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Nothing found when querying for classroom: {message}", ex.Message);
            return Result.Fail("No classroom found");
        }

    }
    
    public async Task<List<GetClassroomsResponseDto>> GetStudentClassroomsById(int studentId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = """
                    SELECT classroom_id AS id, title, description 
                    FROM classroom AS c
                    JOIN student_in_classroom AS sic
                    ON c.classroom_id = sic.classroom_id
                    WHERE student_id = @StudentId;
                    """;
        var classrooms = await con.QueryAsync<GetClassroomsResponseDto>(query, new { StudentId = studentId });

        return classrooms.ToList();
    }

    public async Task<List<GetClassroomsResponseDto>> GetInstructorClassroomsById(int instructorId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "SELECT classroom_id AS id, title, description FROM classroom WHERE owner = @InstructorId;";

        var classrooms = await con.QueryAsync<GetClassroomsResponseDto>(query, new { InstructorId = instructorId });

        return classrooms.ToList();
    }

    public async Task<Result> UpdateClassroomDetailsAsync(UpdateClassroomDto dto, int classroomId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var transaction = con.BeginTransaction();

        var query = """
                    UPDATE classroom 
                    SET title = @Title, description = @Description, registration_open = @RegistrationOpen
                    WHERE classroom_id = @ClassroomId;
                    """;
        var result = await con.ExecuteAsync(query, new 
        { 
            Title = dto.Title, 
            Description = dto.Description, 
            RegistrationOpen = dto.RegistrationOpen, 
            ClassroomId = classroomId 
        }, transaction);

        if(result != 1)
        {
            _logger.LogDebug("Unintended behavior when updateing classroom of id {classroom_id}", classroomId);
            transaction.Rollback();
            return Result.Fail("Failed to update classroom");
        }
        transaction.Commit();

        return Result.Ok();
    }

    public async Task<Result> UpdateClassroomSessionAsync(UpdateClassroomSessionDto dto)
    {
        using var con = await _connection.CreateConnectionAsync();
        var transaction = con.BeginTransaction();

        var updateSessionQuery = """
                                 UPDATE session
                                 SET title = @Title, description = @Description
                                 WHERE session_id = @SessionId;
                                 """;

        var sessionUpdated = await con.ExecuteAsync(updateSessionQuery, new { Title = dto.Title, Description = dto.Description, SessionId = dto.Id }, transaction);
        if(sessionUpdated != 1)
        {
            _logger.LogDebug("Unintended behavior when updateing classroom session of id {classroom_id}", dto.Id);
            transaction.Rollback();
            return Result.Fail("Failed to update classroom session");
        }

        var updateActiveQuery = """
                                UPDATE session_in_classroom
                                SET active = @Active
                                WHERE session_id = @SessionId
                                """;
        var activationsUpdated = await con.ExecuteAsync(updateActiveQuery, new { Active = dto.Active, SessionId = dto.Id }, transaction);

        if(activationsUpdated != 1)
        {
            transaction.Rollback();
            _logger.LogError("Session with id {session_id} failed to update activation status, due to incorrect amount of database rows affected", dto.Id);
            return Result.Fail("Failed to update activation status");
        }

        var updateSessionExercises = """
                                     DELETE FROM exercise_in_session WHERE session_id = @SessionId;
                                     """;

        await con.ExecuteAsync(updateSessionExercises, new { SessionId = dto.Id }, transaction);

        var UpdatedExercises = await _sessionRepository.InsertExerciseRelation(dto.ExerciseIds, dto.Id, con, transaction);
        if (UpdatedExercises.IsFailed)
        {
            transaction.Rollback();
            return Result.Fail("Failed to update exercises of classroom session");
        }

        var updateSessionLangauges = """
                                     DELETE FROM language_in_session WHERE session_id = @SessionId;
                                     """;

        await con.ExecuteAsync(updateSessionLangauges, new { SessionId = dto.Id }, transaction);

        var UpdatedLanguages = await _sessionRepository.InsertLanguageRelation(dto.LanguageIds, dto.Id, con, transaction);
        if (UpdatedLanguages.IsFailed)
        {
            transaction.Rollback();
            return Result.Fail("Failed to update language of classroom session");
        }

        transaction.Commit();

        return Result.Ok();
    }

    public async Task<Result> JoinClassroomAsync(int studentId, int classroomId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var transaction = con.BeginTransaction();

        var query = "INSERT INTO student_in_classroom (student_id, classroom_id) VALUES (@StudentId, @ClassroomId);";

        var result = await con.ExecuteAsync(query, new { StudentId = studentId, ClassroomId = classroomId }, transaction);

        if(result != 1)
        {
            transaction.Rollback();
            _logger.LogWarning("Mismatch in number of students added on a joined classroom. Studentid: {studentId}, Classroom id: {classroomID}", studentId, classroomId);
            return Result.Fail("Failed to join student");
        }
        transaction.Commit();

        return Result.Ok();
    }

    public async Task DeleteClassroomSessionAsync(int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "DELETE FROM session WHERE session_id = @SessionId";

        await con.ExecuteAsync(query, new { SessionId = sessionId });
    }

    public async Task<GetClassroomSessionResponseDto> GetClassroomSessionByIdAsync(int sessionId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = """
                    SELECT
                        s.session_id AS id,
                        s.title AS title,
                        s.description AS description,
                        s.author_id AS authorid,
                        sic.active AS active
                    FROM session s
                    JOIN session_in_classroom sic ON sic.session_id = s.session_id
                    WHERE s.session_id = @SessionId
                    """;

        var session = await con.QuerySingleAsync<GetClassroomSessionResponseDto>(query, new { SessionId = sessionId });

        session.ExerciseIds = await _sessionRepository.GetExercisesOfSessionAsync(sessionId, con);

        var languageQuery = "SELECT language_id FROM language_in_session WHERE session_id = @SessionId;";

        var languages = await con.QueryAsync<Language>(languageQuery, new { SessionId = sessionId });
        session.Languages = languages.ToList();

        return session;
    }

    public async Task<bool> VerifyClassroomAuthor(int classroomId, int authorId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "SELECT 1 FROM classroom WHERE owner = @AuthorId AND classroom_id = @ClassroomId;";

        var result = await con.QuerySingleAsync<int>(query, new { AuthorId = authorId, ClassroomId = classroomId });

        if(result != 1)
        {
            _logger.LogWarning("Missmatch between owner with id {owner_id} and classroom of id {classroom_id}", authorId, classroomId);
            return false;
        }

        return true;
    }

    public async Task<bool> VerifyClassroomRoomcode(int classroomId, string roomCode)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "SELECT roomcode FROM classroom WHERE classroom_id = @ClassroomId;";
        var quriedRoomCode = await con.QuerySingleAsync<string>(query, new { ClassroomId = classroomId });

        return quriedRoomCode == roomCode;
    }

    public async Task<bool> VerifyRegistrationIsOpen(int classroomId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "SELECT registration_open FROM classroom WHERE classroom_id = @ClassroomId;";
        return  await con.ExecuteScalarAsync<bool>(query, new { ClassroomId = classroomId });
    }
}
