using System.Net;
using System.Net.Http.Json;
using Core.Languages.Models;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using FluentResults;
using Infrastructure.Mozart.Strategies;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Mozart;

public class MozartService : IMozartService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<MozartService> _logger;
	
	public MozartService(HttpClient httpClient, ILogger<MozartService> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}
	
	private IMozartStrategy DetermineStrategy(Language language)
    {
        return language switch
        {
            Language.Haskell => new HaskellStrategy(),
            Language.Python => new PythonStrategy(),
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }
	public async Task<Result<SolutionRunnerResponse>> SubmitSubmission(SubmissionDto submission, Language language)
	{
		var url = DetermineStrategy(language).Url;
	    _httpClient.BaseAddress = new Uri(url);
        using var response = await _httpClient.PostAsJsonAsync("/submit", submission);
        _logger.LogInformation("HTTP response: {response}, {body}", response.StatusCode, response.Content.ReadAsStringAsync().Result);

        var mozartResponse = new SolutionRunnerResponse();
        if (response.IsSuccessStatusCode)
        {
        var responseAction = await response.Content.ReadFromJsonAsync<SolutionResponseDto>();
            switch (responseAction?.Result)
            {
                case "pass": 
                    mozartResponse.Action = ResponseCode.Pass;
                    break;
                case "failure":
                    _logger.LogInformation("Testcases failed");
                    mozartResponse.Action = ResponseCode.Failure;
                    break;
                case "error":
                    _logger.LogInformation("Execution of solution failed");
                    mozartResponse.Action = ResponseCode.Error;
                    break;
                default:
                    _logger.LogError("Unknown result received from solution runner: {response}", responseAction);
                    throw new Exception("Unknown result received from solution runner");
            }
            mozartResponse.ResponseBody = await response.Content.ReadAsStringAsync();
            return mozartResponse;
        }
        if (response.StatusCode == HttpStatusCode.UnprocessableContent)
        {
            _logger.LogError("Wrong format of json provided to solution runner:, {message}", submission.Solution);
            return Result.Fail("Error occured");
        }
	    _logger.LogError("Unknown response from solution runner: {response}", response.Content.ReadAsStringAsync().Result);
	    return Result.Fail("Unknown error occured");
    }
}