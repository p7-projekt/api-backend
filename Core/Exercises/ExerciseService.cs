using Core.Exercises.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Core.Exercises.Contracts;
using Core.Solutions.Contracts;

namespace Core.Exercises;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ILogger<ExerciseService> _logger;
    private readonly ISolutionRepository _solutionRepository;

    public ExerciseService(IExerciseRepository exerciseRepository, ILogger<ExerciseService> logger, ISolutionRepository solutionRepository)
    {
        _exerciseRepository = exerciseRepository;
        _logger = logger;
        _solutionRepository = solutionRepository;
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

    public async Task<Result<GetExerciseResponseDto>> GetExerciseById(int exerciseId)
    {
        var exercise = await _exerciseRepository.GetExerciseByIdAsync(exerciseId);
        if (exercise == null) 
        {
            _logger.LogDebug("Failed to retreive exercies with id: {exercise_id}", exerciseId);
            return Result.Fail("Failed to retreive exercise");
        }
        exercise.TestCases = await _solutionRepository.GetTestCasesByExerciseIdAsync(exerciseId) ?? new List<TestCaseEntity>();
        if(exercise.TestCases.Count() == 0)
        {
            _logger.LogDebug("Failed to retreive testcases of exercise with id: {exercise_id}", exerciseId);
            return Result.Fail("Failed to retreive exercise");
        }

        return Result.Ok(exercise);
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
