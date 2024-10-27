using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Exercises.Models;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exercises;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(IExerciseRepository exerciseRepository, ILogger<ExerciseService> logger)
    {
        _exerciseRepository = exerciseRepository;
        _logger = logger;
    }

    public async Task<Result> DeleteExercise(int exerciseId, int userId)
    {
        var result = await _exerciseRepository.VerifyExerciseAuthorAsync(exerciseId, userId);
        if (!result)
        {
            _logger.LogInformation("Exercise: {exercise_id} not created by provided author: {author_Id}", exerciseId, userId);
            return Result.Fail("Exercise not created by provided author.");
        }
        var result2 = await _exerciseRepository.DeleteExerciseAsync(exerciseId);
        if (!result2)
        {
            return Result.Fail("Exercise could not be deleted");
        }
        return Result.Ok();
    }

    public async Task<Result<List<GetExercisesResponseDto>>> GetExercises(int userId)
    {
        var exercises = await _exerciseRepository.GetExercisesAsync(userId);
        if (exercises == null)
        {
            return Result.Fail("Exercises not found");
        }

        return Result.Ok(exercises.ToList());
    }
}
