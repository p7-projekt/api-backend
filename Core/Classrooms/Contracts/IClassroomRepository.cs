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
    Task<bool> VerifyClassroomAuthor(int classroomId, int authorId);
    Task<GetClassroomResponseDto> GetClassroomByIdAsync(int classroomId);
    Task<List<GetClassroomsResponseDto>> GetStudentClassroomsById(int studentId);
    Task<List<GetClassroomsResponseDto>> GetInstructorClassroomsById(int instructorId);
    Task<Result> UpdateClassroomDetailsAsync(UpdateClassroomDto dto, int classroomId);

}
