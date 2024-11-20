using Core.Solutions.Contracts;

namespace Core.Solutions.Strategies;

public class HaskellStrategy : IMozartStrategy
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