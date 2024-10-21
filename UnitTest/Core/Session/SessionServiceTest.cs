using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Shared.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Core.Session;

public class SessionServiceTest
{
    [Fact]
    public void GenerateSessionCode_ShouldReturn_ValidCode()
    {
        // Arrange
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);
        
        // Act
        var result = sessionService.GenerateSessionCode();
        
        // Assert
        Assert.Equal(6, result.Length);
        Assert.Contains(result, x => char.IsLetter(x));
        Assert.Contains(result, x => char.IsDigit(x));
    }
}