using Core.Classrooms.Contracts;
using Core.Classrooms.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Classrooms;

public class ClassroomService : IClassroomService
{
    private readonly ILogger<ClassroomService> _logger;
    private readonly IClassroomRepository _classroomRepository;
    public ClassroomService(ILogger<ClassroomService> logger, IClassroomRepository classroomRepository)
    {
        _logger = logger;
        _classroomRepository = classroomRepository;
    }
    public async Task<Result> CreateClassroom(ClassroomDto dto, int authorId)
    {
        return await _classroomRepository.InsertClassroomAsync(dto, authorId);
    }
}
