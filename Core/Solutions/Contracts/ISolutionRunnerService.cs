using Core.Exercises.Models;
using Core.Solutions.Models;
using FluentResults;

namespace Core.Solutions.Contracts;

public interface ISolutionRunnerService
{
    Task<Result<string>> SubmitSolutionAsync(SubmitSolutionDto dto, int exerciseId, int userId);
    
}
