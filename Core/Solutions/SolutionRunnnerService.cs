﻿using Core.Exercises.Models;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Core.Solutions.Services.TestRunners;
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

    public async Task<Result> CreateSolutionAsync(ExerciseSubmissionDto dto)
    {
        return await _haskellService.SubmitSubmission(new Submission(dto));
    }

    public async Task<Result> SubmitSolutionAsync(SubmitSolutionDto dto, int exerciseId, int userId)
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
            return result;
        }
        
        // Ensure user is part of a given session, and Create a solved relation 
        var inserted = await _solutionRepository.InsertSolvedRelation(userId);
        if (!inserted)
        {
            _logger.LogInformation("Failed to insert solved for userid {userid}, for exercise: {exerciseId}, but exercise passed!", userId, exerciseId);
            return Result.Ok();
        }
        return Result.Ok();
    }
}
