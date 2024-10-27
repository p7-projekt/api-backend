using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Exercises.Models;
using Core.Sessions.Models;

namespace Core.Contracts.Repositories;

public interface IExerciseRepository
{
    Task<Result> InsertExerciseAsync(ExerciseDto dto, int authorId);
    Task<bool> VerifyExerciseAuthorAsync(int exerciseId, int authorId);
    Task<bool> VerifyExerciseIdsAsync(List<int> exerciseIds, int authorId);
    Task<IEnumerable<GetExercisesResponseDto>?> GetExercisesAsync(int authorId);
    Task<bool> DeleteExerciseAsync(int exerciseId);
}
