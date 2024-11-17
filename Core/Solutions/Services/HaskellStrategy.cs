using Core.Solutions.Contracts;

namespace Core.Solutions.Services;

public class HaskellStrategy : ILanguageStrategy
{
	public HaskellStrategy()
	{
		var haskellURL = Environment.GetEnvironmentVariable("MOZART_HASKELL");
        if (string.IsNullOrEmpty(haskellURL))
        {
            throw new NullReferenceException("Haskell environment variable not set");
        }
        Url = $"http://{haskellURL}";
	}
}