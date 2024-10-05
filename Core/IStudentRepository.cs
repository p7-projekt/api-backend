namespace Core;

public interface IStudentRepository
{
    Task<string> GetStudentsAsync();
}