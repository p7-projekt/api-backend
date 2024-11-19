using Core.Languages.Contracts;
using Core.Languages.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Languages.Services;

public class LanguageService : ILanguageService
{
	private readonly ILanguageRepository _languageRepository;
	private readonly ILogger<LanguageService> _logger;
	public LanguageService(ILanguageRepository languageRepository, ILogger<LanguageService> logger)
	{
		_languageRepository = languageRepository;
		_logger = logger;
	}
	public async Task<Result<List<GetLanguagesResponseDto>>> GetLanguages()
	{
		var languages = await _languageRepository.GetLanguagesAsync();
		if (!languages.Any())
		{
			_logger.LogWarning("Error finding languages");
			return Result.Fail("No languages found");
		}
		
		return Result.Ok(languages.Select(l => l.ConvertoToGetLanguagesResponseDto()).ToList());
	}
}