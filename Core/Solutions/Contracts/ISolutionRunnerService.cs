using Core.Exercises.Models;
using FluentResults;

namespace Core.Solutions.Contracts;

public interface ISolutionRunnerService
{
    Task<Result> SubmitSolutionAsync(ExerciseSubmissionDto dto);

}
