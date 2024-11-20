using Core.Languages.Models;

namespace Core.Languages.Contracts;

public interface ILanguageRepository
{
	Task<List<LanguageSupport>> GetLanguagesAsync();
}