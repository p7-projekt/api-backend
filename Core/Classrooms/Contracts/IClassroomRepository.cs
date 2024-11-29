using Core.Classrooms.Models;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms.Contracts;

public interface IClassroomRepository
{
    Task<Result> InsertClassroomAsync(ClassroomDto dto, int authorId, string roomCode);
    Task<Result> AddSessionToClassroomAsync(ClassroomSessionDto dto, int authorId, int classroomId);
    Task<Result> DeleteClassroomAsync(int classroomId);
    Task<Result<GetClassroomResponseDto>> GetClassroomByIdAsync(int classroomId);
    Task<List<GetClassroomsResponseDto>> GetStudentClassroomsById(int studentId);
    Task<List<GetClassroomsResponseDto>> GetInstructorClassroomsById(int instructorId);
    Task<Result> UpdateClassroomDetailsAsync(UpdateClassroomDto dto, int classroomId);
    Task<Result> UpdateClassroomSessionAsync(UpdateClassroomSessionDto dto);
    Task<Result<int>> JoinClassroomAsync(int studentId, string roomcode);
    Task DeleteClassroomSessionAsync(int sessionId);
    Task<GetClassroomSessionResponseDto> GetClassroomSessionByIdAsync(int sessionId);
    Task<Result> LeaveClassroomAsync(int classroomId, int studentId);
    Task<bool> VerifyStudentInClassroom(int classroomId, int studentId);
    Task<bool> VerifyClassroomAuthor(int classroomId, int authorId);
    Task<bool> VerifyClassroomRoomcode(int classroomId, string roomCode);
    Task<bool> VerifyRegistrationIsOpen(int classroomId);

}
