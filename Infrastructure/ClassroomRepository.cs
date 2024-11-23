using Core.Classrooms.Contracts;
using Core.Classrooms.Models;
using Core.Languages.Models;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Dapper;
using FluentResults;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

            var createSessionQuery = "INSERT INTO session (title, description, author_id) VALUES(@Title, @Description, @AuhtorId) RETURNING session_id;";
            var sessionId = await con.QuerySingleAsync<int>(createSessionQuery, new { Titel = dto.Title, Description = dto.Description, AuthorId = authorId }, transaction);

            var addSessionQuery = "INSERT INTO session_in_classroom (classroom_id, session_id, active) VALUES (@ClassroomId, @SessionId, FALSE);";
            await con.ExecuteAsync(addSessionQuery, new { ClassroomId = classroomId, SessionId = sessionId }, transaction);

            await _sessionRepository.InsertExerciseRelation(dto.ExerciseIds, sessionId, con, transaction);
            await _sessionRepository.InsertLanguageRelation(dto.LanguageIds, sessionId, con, transaction);
        } catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError("Error occured during creation of classroom sesssion: {exception}", ex.Message);
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
            var getSessionsQuery = "SELECT session_id FROM session_in_classroom WHERE classroom_id = @ClassroomID;";
            var sessionIds = await con.QueryAsync<List<int>>(getSessionsQuery, new { ClassroomId = classroomId });

            var deleteSessionsQuery = "DELETE FROM session WHERE session_id = ANY(@SessionIds);";
            var removed = await con.ExecuteAsync(deleteSessionsQuery, new { SessionIds = sessionIds }, transaction);
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

    public async Task<GetClassroomResponseDto> GetClassroomByIdAsync(int classroomId)
    {
        using var con = await _connection.CreateConnectionAsync();
        
        var classroomQuery = "SELECT classroom_id AS id, title, roomcode, registration_open AS isOpen FROM classroom WHERE classroom_id = @ClassroomId;";
        var classroom = await con.QuerySingleAsync<GetClassroomResponseDto>(classroomQuery, new { ClassroomId = classroomId });

        var sessionIdsQuery = """
                              SELECT session_id, title, active 
                              FROM session_in_classroom AS sic
                              JOIN session AS s 
                              ON s.session_id = sic.session_id
                              WHERE classroom_id = @ClassroomId
                              """;

        var sessions = await con.QueryAsync<GetClassroomSessionDto>(sessionIdsQuery, new { ClassroomID = classroomId });
        classroom.Sessions = sessions.ToList();

        return classroom;
    }
    
    public async Task<List<GetClassroomsResponseDto>> GetStudentClassroomsById(int studentId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = """
                    SELECT classroom_id AS id, title 
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

        var query = "SELECT classroom_id AS id, title FROM classroom WHERE owner = @InstructorId;";

        var classrooms = await con.QueryAsync<GetClassroomsResponseDto>(query, new { InstructorId = instructorId });

        return classrooms.ToList();
    }

    public async Task<Result> UpdateClassroomDetailsAsync(UpdateClassroomDto dto, int classroomId)
    {
        using var con = await _connection.CreateConnectionAsync();
        var transaction = con.BeginTransaction();

        var query = """
                    UPDATE classroom 
                    SET title = @Title, description = @Description, registration_open = @RegistrartionOpen
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

    public async Task<bool> VerifyClassroomAuthor(int classroomId, int authorId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "SELECT 1 FROM classroom WHERE author_id = @AuthorId AND classroom_id = @ClassroomId;";

        var result = await con.ExecuteAsync(query, new { AuthorId = authorId, ClassroomId = classroomId });

        if(result != 1)
        {
            _logger.LogWarning("Missmatch between owner with id {owner_id} and classroom of id {classroom_id}", authorId, classroomId);
            return false;
        }

        return true;
    }
}
