using Core.Exercises.Models;
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

    public async Task<Result> SubmitSolutionAsync(SubmitSolutionDto dto)
    {
        var testcases = await _solutionRepository.GetTestCasesByExerciseIdAsync(dto.ExerciseId);
        if (testcases == null)
        {
            return Result.Fail("Error retreiving test cases");
        }

        var submission = SubmissionMapper.ToSubmission(testcases, dto.Solution);
        _logger.LogDebug("Dto: {submission}, with testcases: {testcases}, input: {input}, output {output}", submission, submission.TestCases, submission.TestCases.First().InputParameters, submission.TestCases.First().OutputParameters);
        return await _haskellService.SubmitSubmission(submission);
    }
}
