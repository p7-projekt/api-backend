using Core.Classrooms.Contracts;
using Core.Classrooms.Models;
using Dapper;
using FluentResults;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public class ClassroomRepository : IClassroomRepository
{
    private readonly IDbConnectionFactory _connection;
    private readonly ILogger<ClassroomRepository> _logger;
    public ClassroomRepository(ILogger<ClassroomRepository> logger, IDbConnectionFactory connection)
    {
        _logger = logger;
        _connection = connection;
    }

    public async Task<Result> InsertClassroomAsync(ClassroomDto dto, int authorId)
    {
        using var con = await _connection.CreateConnectionAsync();

        var query = "INSERT INTO classroom (title, author_id) VALUES (@Title, @AuthorId) RETURNING classroom_id;";

        var result = await con.ExecuteAsync(query, new { Titel = dto.Title, AuthorId = authorId } );

        if (result == 0)
        {
            _logger.LogWarning("Failed to insert new classroom of user {author_id}", authorId);
            return Result.Fail("Failed to insert new classroom");
        }

        return Result.Ok();
    }
}
