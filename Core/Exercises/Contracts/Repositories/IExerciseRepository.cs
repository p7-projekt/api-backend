using Core.Exercises.Models;
using FluentResults;

namespace Core.Exercises.Contracts.Repositories;

public interface IExerciseRepository
{
    Task<Result> InsertExerciseAsync(ExerciseDto dto, int userId);
    Task<bool> VerifyExerciseIdsAsync(List<int> exerciseIds, int authorId);
}
