using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Exercises.Models;

namespace Core.Contracts.Repositories;

public interface IExerciseRepository
{
    Task<Result> InsertExerciseAsync(ExerciseDto dto, int userId);
    Task<bool> VerifyExerciseIdsAsync(List<int> exerciseIds, int authorId);
}
