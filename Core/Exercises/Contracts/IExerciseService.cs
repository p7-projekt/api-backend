using Core.Exercises.Models;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exercises.Contracts;

public interface IExerciseService
{
    Task<Result<List<GetExercisesResponseDto>>> GetExercises(int userId);
    Task<Result> DeleteExercise(int exerciseId, int userId);
    Task<Result<GetExerciseResponseDto>> GetExerciseById(int exerciseId);
    Task<Result> UpdateExercise(int exerciseId, int authorId, ExerciseDto dto);
}
