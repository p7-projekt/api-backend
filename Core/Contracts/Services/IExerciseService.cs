using Core.Exercises.Models;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Services;

public interface IExerciseService
{
    Task<Result<List<GetExercisesResponseDto>>> GetExercises(int userId);
    Task<Result> DeleteExercise(int exerciseId, int userId);
}
