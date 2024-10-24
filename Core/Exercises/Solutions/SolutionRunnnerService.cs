using System.Text;
using System.Text.Json;
using Core.Exercises.Contracts.Services;
using Core.Exercises.Models;
using Core.Exercises.Services.TestRunners;
using Microsoft.Extensions.Logging;

namespace Core.Exercises.Solutions;

public class SolutionRunnnerService : ISolutionRunnerService
{
    private readonly ILogger<SolutionRunnnerService> _logger;
    private readonly HaskellService _haskellService;

    public SolutionRunnnerService(ILogger<SolutionRunnnerService> logger, HaskellService haskellService)
    {
        _logger = logger;
        _haskellService = haskellService;
    }

    public async Task SubmitSolutionAsync(ExerciseDto dto)
    {
        await _haskellService.SubmitSolution(new Submission(dto));
        // var haskellURL = Environment.GetEnvironmentVariable("MOZART_HASKELL");
        // if (string.IsNullOrEmpty(haskellURL))
        // {
        //     throw new NullReferenceException("Haskell environment variable not set");
        // }
        // var url = $"http://{haskellURL}/submit";
        // using var client = new HttpClient();
        //
        // var submission = JsonSerializer.Serialize(new Submission(dto));
        //
        // _logger.LogDebug("Current submission: {submission}", submission);
        //
        //
        // var content = new StringContent(submission, Encoding.UTF8, "application/json");
        // var response = await client.PostAsync(url, content);
        // string responseBody = await response.Content.ReadAsStringAsync();
        // _logger.LogInformation("Response: {response}", responseBody);
        //Managge potentiel responses from solution runner.
    }
}
