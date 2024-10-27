using Core.Exercises.Models;
using FluentResults;

namespace Core.Contracts.Services;

public interface ISolutionRunnerService
{
    Task<Result> SubmitSolutionAsync(ExerciseSubmissionDto dto);

}
