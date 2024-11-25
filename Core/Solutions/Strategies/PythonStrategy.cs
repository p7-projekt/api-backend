using Core.Solutions.Contracts;

namespace Core.Solutions.Strategies;

public class PythonStrategy : IMozartStrategy
{
    public PythonStrategy()
    {
        var pythonURL = Environment.GetEnvironmentVariable("MOZART_PYTHON");
        if (string.IsNullOrEmpty(pythonURL))
        {
            throw new NullReferenceException("Python environment variable not set");
        }
        Url = $"http://{pythonURL}";
    }
}