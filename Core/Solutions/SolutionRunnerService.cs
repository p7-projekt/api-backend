using Core.Solutions.Contracts;
using Core.Solutions.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Solutions;

public class SolutionRunnerService : ISolutionRunnerService
{
    private readonly ILogger<SolutionRunnerService> _logger;
    private readonly ISolutionRepository _solutionRepository;
    private readonly ILanguageService _languageService;

    public SolutionRunnerService(ILogger<SolutionRunnerService> logger, ISolutionRepository solutionRepository, ILanguageService languageService)
    {
        _logger = logger;
        
        _solutionRepository = solutionRepository;
        _languageService = languageService;
    }

    public async Task<Result<HaskellResponseDto>> SubmitSolutionAsync(SubmitSolutionDto dto, int exerciseId, int userId)
    {
        // validate anon user is part of a given session
        // Short circuit if user is not part of the session
        var userExistsInSession = await _solutionRepository.CheckAnonUserExistsInSessionAsync(userId, dto.SessionId);
        if (!userExistsInSession)
        {
            _logger.LogWarning("User {UserId} does not exist in Session {SessionId}", userId, dto.SessionId);
            return Result.Fail($"User {userId} does not exist in the session.");
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
        var submission = SubmissionMapper.ToSubmission(testcases, dto.Solution);
        var result = await _languageService.SubmitSubmission(submission, (Language)language.Id);
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
