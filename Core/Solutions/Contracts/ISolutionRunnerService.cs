using Core.Exercises.Models;
using Core.Solutions.Models;
using FluentResults;

namespace Core.Solutions.Contracts;

public interface ISolutionRunnerService
{
    Task<Result<dynamic?>> SubmitSolutionAsync(SubmitSolutionDto dto, int exerciseId, int userId);
    
}
