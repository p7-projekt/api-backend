using Core.Sessions;

namespace UnitTest.Core.Session;

public class SessionServiceTest
{
    [Fact]
    public void GenerateSessionCode_ShouldReturn_ValidCode()
    {
        // Arrange
        var sessionService = new SessionService();
        
        // Act
        var result = sessionService.GenerateSessionCode();
        
        // Assert
        Assert.Equal(6, result.Length);
        Assert.Contains(result, x => char.IsLetter(x));
        Assert.Contains(result, x => char.IsDigit(x));
    }
}