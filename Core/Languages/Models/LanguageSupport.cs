namespace Core.Languages.Models;

public class LanguageSupport
{
	public int Id { get; set; }
	public string Language { get; set; } = string.Empty;
	public string Version { get; set; } = string.Empty;
}

public static class LanguageSupportMapper
{
	public static GetLanguagesResponseDto ConvertoToGetLanguagesResponseDto(this LanguageSupport languageSupport)
	{
		return new GetLanguagesResponseDto(languageSupport.Id, languageSupport.Language);
	}
}