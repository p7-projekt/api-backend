using Core.Dashboards;
using Core.Dashboards.Contracts;
using Core.Exercises.Contracts;
using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using Core.Shared.Contracts;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Core.Sessions;

public class DashboardServiceTest
{
    [Fact]
    public void GetExercisesInSession_Timed_ShouldReturn_GetExercisesCombinedInfoDto()
    {
        // Arrange
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        // Act
        var result = dashboardService.GetExercisesInSession()

        // Assert
        Assert.Equal(6, result.Length);
        Assert.Contains(result, x => char.IsLetter(x));
        Assert.Contains(result, x => char.IsDigit(x));
    }
}