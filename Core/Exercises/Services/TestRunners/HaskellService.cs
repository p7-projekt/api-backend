using System.Net.Http.Json;
using Core.Exercises.Models;
using Microsoft.Extensions.Logging;

namespace Core.Exercises.Services.TestRunners;

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

	public async Task<string?> SubmitSolution(Submission dto)
	{
		using var response = await _httpClient.PostAsJsonAsync("/submit", dto);

		_logger.LogInformation("HTTP response: {response}, {body}", response.StatusCode, response.Content.ReadAsStringAsync().Result);
		return "";
	} 
}