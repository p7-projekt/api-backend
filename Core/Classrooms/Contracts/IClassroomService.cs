using Core.Classrooms.Models;
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


}
