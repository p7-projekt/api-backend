using Core.Exercises.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Core.Exercises.Contracts;
using Core.Languages.Models;
using Core.Solutions;
using Core.Solutions.Contracts;
using Core.Solutions.Models;

namespace Core.Exercises;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ILogger<ExerciseService> _logger;
    private readonly ISolutionRepository _solutionRepository;
    private readonly IMozartService _iMozartService;


    public ExerciseService(IExerciseRepository exerciseRepository, ILogger<ExerciseService> logger, ISolutionRepository solutionRepository, IMozartService iMozartService)
    {
        _exerciseRepository = exerciseRepository;
        _logger = logger;
        _solutionRepository = solutionRepository;
        _iMozartService = iMozartService;
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
        var tempTestcases = await _solutionRepository.GetTestCasesByExerciseIdAsync(exerciseId) ?? new List<Testcase>();
        exercise.TestCases = tempTestcases.Select(x => x.ToTestcaseDto()).ToList();
        if (exercise.TestCases.Count() == 0)
        {
            _logger.LogDebug("Failed to retreive testcases of exercise with id: {exercise_id}", exerciseId);
            return Result.Fail("Failed to retreive exercise");
        }
        exercise.InputParameterType = tempTestcases.First().Input.Select(x => x.ParameterType).ToList();
        exercise.OutputParamaterType = tempTestcases.First().Output.Select(x => x.ParameterType).ToList();

        return Result.Ok(exercise);
    }

    public async Task<Result<List<GetExercisesResponseDto>>> GetExercises(int userId)
    {
        var exercises = await _exerciseRepository.GetExercisesAsync(userId);
        if (exercises == null || exercises.Count() == 0)
        {
            return Result.Fail("Exercises not found");
        }

        return Result.Ok(exercises.ToList());
    }

    public async Task<Result<SolutionRunnerResponse>> UpdateExercise(int exerciseId, int authorId, ExerciseDto dto)
    {
        var result = await _exerciseRepository.VerifyExerciseAuthorAsync(exerciseId, authorId);
        if (!result)
        {
            _logger.LogInformation("Exercise: {exercise_id} not created by provided author: {author_Id}", exerciseId, authorId);
            return Result.Fail("Exercise not updated");
        }

        var submissionResult = await _iMozartService.SubmitSubmission(new SubmissionDto(dto), (Language)dto.SolutionLanguage);

        if (submissionResult.IsFailed) 
        {
            _logger.LogInformation("Failed to validate exercise: {exercise}", dto);
            return Result.Fail("Internal error occurred on solution runner");
        }

        if (!submissionResult.Value.Action.Equals(ResponseCode.Pass))
        {
            return submissionResult;
        }

        var updateResult = await _exerciseRepository.UpdateExerciseAsync(dto, exerciseId);
        if (updateResult.IsFailed) {
            _logger.LogInformation("Failed to update exercise with id: {exercise_id}", exerciseId);
            return Result.Fail("Exercise not updated");
        }

        return submissionResult;
    }

    public async Task<Result<MozartResponseDto>> CreateExercise(ExerciseDto dto, int authorId)
    {
        var result = await _iMozartService.SubmitSubmission(new SubmissionDto(dto), (Language)dto.SolutionLanguage);
        if (result.IsFailed)
        {
            return Result.Fail("Internal error");
        }

        switch (result.Value.Action)
        {
            case ResponseCode.Failure:
                return Result.Ok(new MozartResponseDto(result.Value.ResponseDto!.TestCaseResults, null));
            case ResponseCode.Error:
                return Result.Ok(new MozartResponseDto(null, result.Value.ResponseDto!.Message));
        }

        var insertResult = await _exerciseRepository.InsertExerciseAsync(dto, authorId);
        if(insertResult.IsFailed)
        {
            _logger.LogError("Failed to insert exercise: {exercise}", dto);
            return Result.Fail("Internal error");
        }

        return Result.Ok();
    }
}
