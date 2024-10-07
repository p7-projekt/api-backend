using System.Data;
using Core;
using Core.Shared.Contracts;
using Dapper;

namespace Infrastructure;

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnectionFactory _connection;

    public StudentRepository(IDbConnectionFactory connection)
    {
        _connection = connection;
    }

    public async Task<string> GetStudentsAsync()
    {
        // check connection can be done on prod 
        using var con = await _connection.CreateConnectionAsync();
        if (con.State == ConnectionState.Open)
        {
            Console.WriteLine("Connection established");
        }
        // noget postgres connection select * students og return 
        Console.WriteLine("Get students is triggerd");
        return "Hello";
    }
}