﻿using Core.Exercises.Models;
using FluentResults;

namespace Core.Exercises.Contracts.Repositories;

public interface IExerciseRepository
{
    Task<Result> InsertExerciseAsync(ExerciseDto dto, int userId);
    Task<bool> VerifyExerciseAuthorAsync(int exerciseId, int authorId);
    Task<IEnumerable<GetExercisesResponseDto>?> GetExercisesAsync(int authorId);
    Task<bool> DeleteExerciseAsync(int exerciseId);
    Task<bool> VerifyExerciseIdsAsync(List<int> exerciseIds, int authorId);
}