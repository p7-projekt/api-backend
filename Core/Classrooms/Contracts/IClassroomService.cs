using Core.Classrooms.Models;
using Core.Shared;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Contracts;

public interface IClassroomService
{
    Task<Result> CreateClassroom(ClassroomDto dto, int authorId);
    Task<Result> AddSessionToClassroom(ClassroomSessionDto dto, int authorId, int classroomId);
    Task<Result> DeleteClassroom(int classroomId, int authorId);
    Task<Result<GetClassroomResponseDto>> GetClassroomById(int classroomId, int userID, Roles userRole);
    Task<Result<List<GetClassroomsResponseDto>>> GetClassroomsByUserRole(int userId, Roles userRole);
    Task<Result> UpdateClassroomDetails(UpdateClassroomDto dto, int classroomId, int authorId);
    Task<Result> UpdateClassroomSession(UpdateClassroomSessionDto dto, int classroomId, int authorId);
    Task<Result> DeleteClassroomSession(int sessionId, int authorId);
    Task<GetClassroomSessionResponseDto> GetClassroomSessionById(int sessionId, int userId);
    Task<Result> LeaveClassroom(int classroomId, int studentId);
}
