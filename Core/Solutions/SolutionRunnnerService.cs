using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Core.Solutions.Services;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Solutions;

public class SolutionRunnnerService : ISolutionRunnerService
{
    private readonly ILogger<SolutionRunnnerService> _logger;
    private readonly HaskellService _haskellService;
    private readonly ISolutionRepository _solutionRepository;

    public SolutionRunnnerService(ILogger<SolutionRunnnerService> logger, HaskellService haskellService, ISolutionRepository solutionRepository)
    {
        _logger = logger;
        _haskellService = haskellService;
        _solutionRepository = solutionRepository;
    }

    // public async Task<Result<>> ConfirmSolutionAsync(ExerciseDto dto)
    // {
    //     var result = await _haskellService.SubmitSubmission(new SubmissionDto(dto));
    //     if (result.IsFailed)
    //     {
    //         return result;
    //     }
    //     return await _haskellService.SubmitSubmission(new SubmissionDto(dto));
    // }

    public async Task<Result<HaskellResponseDto>> SubmitSolutionAsync(SubmitSolutionDto dto, int exerciseId, int userId)
    {
        // validate anon user is part of a given session
        // Short circuit if user is not part of the session
        var userExistsInSession = await _solutionRepository.CheckAnonUserExistsInSessionAsync(userId);
        if (!userExistsInSession)
        {
            _logger.LogWarning("User {UserId} does not exist in Session {SessionId}", userId, dto.SessionId);
            return Result.Fail($"User {userId} does not exist in the session.");
        }
        // get testcases
        var testcases = await _solutionRepository.GetTestCasesByExerciseIdAsync(exerciseId);
        if (testcases == null)
        {
            return Result.Fail("Error retreiving test cases");
        }
        
        // Validate through mozart
        var submission = SubmissionMapper.ToSubmission(testcases, dto.Solution);
        var result = await _haskellService.SubmitSubmission(submission);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }

        if (result.Value.Action == ResponseCode.Failure)
        {
            return new HaskellResponseDto(result.Value.ResponseDto!.TestCaseResults, null);
        }

        if (result.Value.Action == ResponseCode.Error)
        {
            return new HaskellResponseDto(null, result.Value.ResponseDto!.Message);
        }
        
        
        // Ensure user is part of a given session, and Create a solved relation 
        var inserted = await _solutionRepository.InsertSolvedRelation(userId, exerciseId);
        if (!inserted)
        {
            _logger.LogInformation("Failed to insert solved for userid {userid}, for exercise: {exerciseId}, but exercise passed!", userId, exerciseId);
            return Result.Ok();
        }
        return Result.Ok();
    }
}
