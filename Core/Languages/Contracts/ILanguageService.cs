using Core.Languages.Models;
using FluentResults;

namespace Core.Languages.Contracts;

public interface ILanguageService
{
	Task<Result<List<GetLanguagesResponseDto>>> GetLanguages();
}