using Core.Exercises.Models;
using Core.Solutions.Contracts;
using Core.Solutions.Services.TestRunners;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Solutions;

public class SolutionRunnnerService : ISolutionRunnerService
{
    private readonly ILogger<SolutionRunnnerService> _logger;
    private readonly HaskellService _haskellService;

    public SolutionRunnnerService(ILogger<SolutionRunnnerService> logger, HaskellService haskellService)
    {
        _logger = logger;
        _haskellService = haskellService;
    }

    public async Task<Result> SubmitSolutionAsync(ExerciseSubmissionDto dto)
    {
        return await _haskellService.CreateSolution(dto);
    }
}
