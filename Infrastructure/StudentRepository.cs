using Core;

namespace Infrastructure;

public class StudentRepository : IStudentRepository
{
    public Task<string> GetStudentsAsync()
    {
        // noget postgres connection select * students og return 
        Console.WriteLine("Get students is triggerd");
        return Task.FromResult("Hello");
    }
}