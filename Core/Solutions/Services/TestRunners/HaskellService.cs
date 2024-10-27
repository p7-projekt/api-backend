using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Exercises.Models;
using Core.Solutions.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Solutions.Services.TestRunners;

public class HaskellService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<HaskellService> _logger;

	public HaskellService(HttpClient httpClient, ILogger<HaskellService> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
		var haskellURL = Environment.GetEnvironmentVariable("MOZART_HASKELL");
		if (string.IsNullOrEmpty(haskellURL))
		{
			throw new NullReferenceException("Haskell environment variable not set");
		}
		_httpClient.BaseAddress = new Uri($"http://{haskellURL}");
	}

	public async Task<Result> SubmitSolution(ExerciseSubmissionDto dto)
	{
		using var response = await _httpClient.PostAsJsonAsync("/submit", new Submission(dto));
		_logger.LogInformation("HTTP response: {response}, {body}", response.StatusCode, response.Content.ReadAsStringAsync().Result);
		
		var responseBody = await response.Content.ReadAsStringAsync();
		
		if (response.IsSuccessStatusCode) {
		    var result = JsonDocument.Parse(responseBody).RootElement.GetProperty("result");
		    switch (result.ToString())
		    {
		        case "pass": return Result.Ok();
		        case "failure":
		            _logger.LogInformation("Testcases failed");
		            var reason = JsonDocument.Parse(responseBody).RootElement.GetProperty("reason");
		            return Result.Fail(reason.ToString());
		        case "error":
		            _logger.LogInformation("Execution of solution failed");
		            var errorReason = JsonDocument.Parse(responseBody).RootElement.GetProperty("reason");
		            return Result.Fail(errorReason.ToString());
		        default:
		            _logger.LogError("Unknown result received from solution runner: {response}", responseBody);
		            throw new Exception("Unknown result received from solution runner");
		    }
		} else if(response.StatusCode == HttpStatusCode.UnprocessableContent)
		{
		    _logger.LogError("Wrong format of json provided to solution runner:, {message}", response.Content);
		    return Result.Fail("Wrong format provided to solution runner");
		} else
		{
		    _logger.LogError("Unknown response from solution runner: {response}", responseBody);
		    return Result.Fail("Uknown response encountered");
		}
	}
}