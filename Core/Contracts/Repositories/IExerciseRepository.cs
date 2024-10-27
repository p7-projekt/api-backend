using FluentResults;
using Core.Exercises.Models;

namespace Core.Contracts.Repositories;

public interface IExerciseRepository
{
    Task<Result> InsertExerciseAsync(ExerciseDto dto, int userId);
    Task<bool> VerifyExerciseIdsAsync(List<int> exerciseIds, int authorId);
}
