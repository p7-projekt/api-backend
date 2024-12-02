using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Solutions;

public class SolutionRunnerService : ISolutionRunnerService
{
    private readonly ILogger<SolutionRunnerService> _logger;
    private readonly ISolutionRepository _solutionRepository;
    private readonly IMozartService _iMozartService;

    public SolutionRunnerService(ILogger<SolutionRunnerService> logger, ISolutionRepository solutionRepository, IMozartService iMozartService)
    {
        _logger = logger;
        
        _solutionRepository = solutionRepository;
        _iMozartService = iMozartService;
    }

    public async Task<Result<string>> SubmitSolutionAsync(SubmitSolutionDto dto, int exerciseId, int userId)
    {
        // validate anon user is part of a given session
        // Short circuit if user is not part of the session
        var userExistsInSession = await _solutionRepository.CheckUserAssociationToSessionAsync(userId, dto.SessionId);
        if (!userExistsInSession)
        {
            _logger.LogWarning("User {UserId} does not assocaited to Session {SessionId}", userId, dto.SessionId);
            return Result.Fail($"User {userId} not associated to the session.");
        }
        // get selected language 
        var language = await _solutionRepository.GetSolutionLanguageBySession(dto.LanguageId, dto.SessionId);
        if (language == null)
        {
            _logger.LogWarning("Language {Language} does not exist for exercise id {exerciseid}", dto.LanguageId, exerciseId);
            return Result.Fail($"Language {dto.LanguageId} does not exist.");
        }
        
        // get testcases
        var testcases = await _solutionRepository.GetTestCasesByExerciseIdAsync(exerciseId);
        if (testcases == null)
        {
            return Result.Fail("Error retreiving test cases");
        }
        
        // Validate through mozart
        var submissionResponse = await ValidateInMozart(testcases, dto);
        bool solved = submissionResponse is { IsSuccess: true, Value: null };
        var submission = new Submission
        {
            UserId = userId,
            SessionId = dto.SessionId,
            ExerciseId = exerciseId,
            Solution = solved ? dto.Solution! : null,
            LanguageId = language.Id,
            Solved = solved 
        };
        
        var inserted = await _solutionRepository.InsertSubmissionRelation(submission);
        if (!inserted)
        {
            return Result.Fail("Error inserting submission");
        }
        if (!solved)
        {
            return submissionResponse;
        }
        return Result.Ok();
    }

    private async Task<Result<string>> ValidateInMozart(List<Testcase> testcases, SubmitSolutionDto dto)
    {
        var submission = SubmissionMapper.ToSubmission(testcases, dto.Solution);
        var result = await _iMozartService.SubmitSubmission(submission, (Language)dto.LanguageId);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }

        if (result.Value.Action is ResponseCode.Failure or ResponseCode.Error)
        {
            return result.Value.ResponseBody;
        }

        return Result.Ok();
    }
   
}
