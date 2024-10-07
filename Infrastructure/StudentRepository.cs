using System.Data;
using Core;
using Npgsql;

namespace Infrastructure;

public class StudentRepository : IStudentRepository
{
    public Task<string> GetStudentsAsync()
    {
        // check connection can be done on prod 
        var con = new NpgsqlConnection(Environment.GetEnvironmentVariable("CONNECTIONSTRING"));
        con.Open();
        if (con.State == ConnectionState.Open)
        {
            Console.WriteLine("Connection established");
        }
        con.Close();
        // noget postgres connection select * students og return 
        Console.WriteLine("Get students is triggerd");
        return Task.FromResult("Hello");
    }
}