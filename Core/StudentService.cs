namespace Core;

public class StudentService
{
    
    readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }


    public async Task HentNogleStudents()
    {
        await _studentRepository.GetStudentsAsync();
    }
}