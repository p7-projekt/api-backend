using Core.Exercises.Models;
using Core.Solutions.Models;
using FluentResults;

namespace Core.Solutions.Contracts;

public interface ISolutionRunnerService
{
    // Task<Result> ConfirmSolutionAsync(ExerciseDto dto);
    Task<Result<HaskellResponseDto>> SubmitSolutionAsync(SubmitSolutionDto dto, int exerciseId, int userId);
    
}
