using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Exercises.Models;
using Core.Solutions.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Solutions.Services;

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
    public async Task<Result<HaskellResponse>> SubmitSubmission(SubmissionDto submission)
    {
        using var response = await _httpClient.PostAsJsonAsync("/submit", submission);
        _logger.LogInformation("HTTP response: {response}, {body}", response.StatusCode, response.Content.ReadAsStringAsync().Result);

        var haskellResponse = new HaskellResponse();
        if (response.IsSuccessStatusCode)
        {
        var responseBody = await response.Content.ReadFromJsonAsync<MozartHaskellResponseDto>();
            switch (responseBody?.Result)
            {
                case "pass": 
                    haskellResponse.Action = ResponseCode.Pass;
                    break;
                case "failure":
                    _logger.LogInformation("Testcases failed");
                    haskellResponse.Action = ResponseCode.Failure;
                    break;
                case "error":
                    _logger.LogInformation("Execution of solution failed");
                    haskellResponse.Action = ResponseCode.Error;
                    break;
                default:
                    _logger.LogError("Unknown result received from solution runner: {response}", responseBody);
                    throw new Exception("Unknown result received from solution runner");
            }
            haskellResponse.ResponseDto = responseBody;
            return haskellResponse;
        }
        else if (response.StatusCode == HttpStatusCode.UnprocessableContent)
        {
            _logger.LogError("Wrong format of json provided to solution runner:, {message}", submission.Solution);
            return Result.Fail("Invalid response.");
        }
        else
        {
            _logger.LogError("Unknown response from solution runner: {response}", response.Content.ReadAsStringAsync().Result);
            return Result.Fail("Invalid reponse");
        }
    }
}