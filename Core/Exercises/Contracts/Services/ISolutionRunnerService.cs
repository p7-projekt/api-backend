using Core.Exercises.Models;

namespace Core.Exercises.Contracts.Services;

public interface ISolutionRunnerService
{
    Task SubmitSolutionAsync(ExerciseDto dto);

}
