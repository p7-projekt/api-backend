using Core.Classrooms.Contracts;
using Core.Classrooms.Models;
using Core.Sessions.Contracts;
using Core.Shared;
using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms;

public class ClassroomService : IClassroomService
{
    private readonly ILogger<ClassroomService> _logger;
    private readonly IClassroomRepository _classroomRepository;
    private readonly ISessionRepository _sessionRepository;
    public ClassroomService(ILogger<ClassroomService> logger, IClassroomRepository classroomRepository, ISessionRepository sessionRepository)
    {
        _logger = logger;
        _classroomRepository = classroomRepository;
        _sessionRepository = sessionRepository;
    }
    public async Task<Result> CreateClassroom(ClassroomDto dto, int authorId)
    {
        var roomCode = GenerateClassroomCode();

        return await _classroomRepository.InsertClassroomAsync(dto, authorId, roomCode);
    }

    public async Task<Result> AddSessionToClassroom(ClassroomSessionDto dto, int authorId, int classroomId)
    {
        var correctAuthor = await _classroomRepository.VerifyClassroomAuthor(classroomId, authorId);
        if (!correctAuthor)
        {
            return Result.Fail("Failed to validate author of classroom");
        }

        return await _classroomRepository.AddSessionToClassroomAsync(dto, authorId, classroomId);
    }

    public async Task<Result> DeleteClassroom(int classroomId, int authorId)
    {
        var correctAuthor = await _classroomRepository.VerifyClassroomAuthor(classroomId, authorId);
        if (!correctAuthor)
        {
            _logger.LogWarning("Invalid user with id {userId} tried to delete classroom with id {classroomId}", authorId, classroomId);
            return Result.Fail("Invalid author");
        }
        return await _classroomRepository.DeleteClassroomAsync(classroomId);
    }

    public async Task<Result<GetClassroomResponseDto>> GetClassroomById(int classroomId)
    {
        return await _classroomRepository.GetClassroomByIdAsync(classroomId);
    }

    public async Task<Result<List<GetClassroomsResponseDto>>> GetClassroomsByUserRole(int userId, Roles userRole)
    {
        var result = userRole switch
        {
            Roles.Student => Result.Ok(await _classroomRepository.GetStudentClassroomsById(userId)),
            Roles.Instructor => Result.Ok(await _classroomRepository.GetInstructorClassroomsById(userId)),
            _ => Result.Fail("Invalid role")
        };

        return result;
    }
    
    public async Task<Result> UpdateClassroomDetails(UpdateClassroomDto dto, int classroomId, int authorId)
    {
        var correctOwner = await _classroomRepository.VerifyClassroomAuthor(classroomId, authorId);

        if (!correctOwner)
        {
            _logger.LogWarning("Invalid user with id {userId} tried to update classroom with id {classroomId}", authorId, classroomId);
            return Result.Fail("Invalid author");
        }

        return await _classroomRepository.UpdateClassroomDetailsAsync(dto, classroomId);
    }

    public async Task<Result> UpdateClassroomSession(UpdateClassroomSessionDto dto, int classroomId, int authorId)
    {
        var correctOwner = await _classroomRepository.VerifyClassroomAuthor(classroomId, authorId);
        if (!correctOwner)
        {
            _logger.LogWarning("Invalid user with id {userId} tried to update classroom session with id {sessionId}, of classroom with id {classroomId}", authorId, classroomId, dto.Id);
            return Result.Fail("Invalid author");
        }

        return await _classroomRepository.UpdateClassroomSessionAsync(dto);
    }

    public async Task<Result> JoinClassroom(JoinClassroomDto dto, int classroomId, int studentId)
    {
        var isOpen = await _classroomRepository.VerifyRegistrationIsOpen(classroomId);
        if (!isOpen)
        {
            return Result.Fail("Classroom not open to join");
        }

        var validCode = await _classroomRepository.VerifyClassroomRoomcode(classroomId, dto.RoomCode);
        if (!validCode)
        {
            _logger.LogWarning("Roomcode {roomcode} for classroom with id {classroomId is invalid}", dto.RoomCode, classroomId);
            return Result.Fail("Incorrect roomcode");
        }
        
        var result = await _classroomRepository.JoinClassroomAsync(studentId, classroomId);
        if (result.IsFailed)
        {

            return Result.Fail("Failed to join classroom");
        }

        return Result.Ok();
    }

    public async Task<Result> DeleteClassroomSession(int sessionId, int authorId)
    {
        var correctAuthor = await _sessionRepository.VerifyAuthor(authorId, sessionId);
        if (!correctAuthor)
        {
            _logger.LogWarning("Invalid owner tried to delete session");
            return Result.Fail("Incorrect author of session");
        }
        await _classroomRepository.DeleteClassroomSessionAsync(sessionId);

        return Result.Ok();
    }

    public async Task<Result> LeaveClassroom(int classroomId, int studentId)
    {
        var inClassroom = await _classroomRepository.VerifyStudentInClassroom(classroomId, studentId);
        if (!inClassroom)
        {
            _logger.LogWarning("Student {studentId} tried to leave classroom {classroomId}, but is not part of classroom", studentId, classroomId);
            return Result.Fail("User not part of classroom already");
        }
        return await _classroomRepository.LeaveClassroomAsync(classroomId, studentId);
    }

    public async Task<GetClassroomSessionResponseDto> GetClassroomSessionById(int sessionId)
    {
        return await _classroomRepository.GetClassroomSessionByIdAsync(sessionId);
    }

    private string GenerateClassroomCode()
    {
        Random rnd = new Random();
        var pinCode = rnd.Next(1000, 10000);
        var secondToLastChar = (char)rnd.Next(65, 91);
        var lastChar = (char)rnd.Next(65, 91);
        var chars = string.Concat(secondToLastChar, lastChar);
        return string.Concat(pinCode.ToString(), chars);
    }
}
