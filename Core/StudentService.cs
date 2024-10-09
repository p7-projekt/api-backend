namespace Core;

public class StudentService
{
    
    readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }


    public Task HentNogleStudents()
    {
        throw new ArgumentException("test");
        // await _studentRepository.GetStudentsAsync();
    }
}