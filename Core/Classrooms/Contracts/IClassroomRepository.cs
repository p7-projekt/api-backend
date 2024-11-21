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

}
