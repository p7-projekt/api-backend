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
}
