using Core.Dashboards;
using Core.Dashboards.Contracts;
using Core.Dashboards.Models;
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
    public async void GetExercisesInSession_TimedSession_ShouldReturn_OK()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardRepoSub.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(false);
        dashboardRepoSub.GetConnectedTimedUsersAsync(Arg.Any<int>()).Returns(4);
        dashboardRepoSub.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(createExerciseResponse());
        // Act
        var result = await dashboardService.GetExercisesInSession(1, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<GetExercisesInSessionCombinedInfo>(result.Value);
    }
    [Fact]
    public async void GetExercisesInSession_TimedSession_NoExercisesFound_ShouldReturn_Fail()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardRepoSub.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(false);
        dashboardRepoSub.GetConnectedTimedUsersAsync(Arg.Any<int>()).Returns(4);
        dashboardRepoSub.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(new List<GetExercisesInSessionResponseDto>());
        // Act
        var result = await dashboardService.GetExercisesInSession(1, 1);

        // Assert
        Assert.True(result.IsFailed);
    }
    [Fact]
    public async void GetExercisesInSession_ClassSession_ShouldReturn_OK()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardRepoSub.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(true);
        dashboardRepoSub.GetConnectedUsersClassAsync(Arg.Any<int>()).Returns(4);
        dashboardRepoSub.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(createExerciseResponse());
        // Act
        var result = await dashboardService.GetExercisesInSession(1, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<GetExercisesInSessionCombinedInfo>(result.Value);
    }
    [Fact]
    public async void GetExercisesInSession_ClassSession_NoExercisesFound_ShouldReturn_Fail()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardRepoSub.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(true);
        dashboardRepoSub.GetConnectedUsersClassAsync(Arg.Any<int>()).Returns(4);
        dashboardRepoSub.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(new List<GetExercisesInSessionResponseDto>());
        // Act
        var result = await dashboardService.GetExercisesInSession(1, 1);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async void GetExercisesInSession_NotAuthor_ShouldReturn_Fail()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        dashboardRepoSub.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(true);
        dashboardRepoSub.GetConnectedUsersClassAsync(Arg.Any<int>()).Returns(4);
        dashboardRepoSub.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(new List<GetExercisesInSessionResponseDto>());
        // Act
        var result = await dashboardService.GetExercisesInSession(1, 1);

        // Assert
        Assert.True(result.IsFailed);
    }
    [Fact]
    public async void GetUserSolution_ShouldReturn_OK()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        dashboardRepoSub.CheckAuthorizedToGetSolution(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardRepoSub.GetSolutionByUserIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(new GetExerciseSolutionResponseDto("","","","",1));

        // Act
        var result = await dashboardService.GetUserSolution(1,1,1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<GetExerciseSolutionResponseDto>(result.Value);
    }
    [Fact]
    public async void GetUserSolution_NotAuthor_ShouldReturn_Fail()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        dashboardRepoSub.CheckAuthorizedToGetSolution(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        dashboardRepoSub.GetSolutionByUserIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(new GetExerciseSolutionResponseDto("", "", "", "", 1));

        // Act
        var result = await dashboardService.GetUserSolution(1, 1, 1);

        // Assert
        Assert.True(result.IsFailed);
    }
    [Fact]
    public async void GetUserSolution_InvalidId_ShouldReturn_Fail()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        dashboardRepoSub.CheckAuthorizedToGetSolution(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardRepoSub.GetSolutionByUserIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Fail(""));

        // Act
        var result = await dashboardService.GetUserSolution(1, 1, 1);

        // Assert
        Assert.True(result.IsFailed);
    }




    private IEnumerable<GetExercisesInSessionResponseDto> createExerciseResponse()
    {
        return new List<GetExercisesInSessionResponseDto>
        {
        new GetExercisesInSessionResponseDto
        {
            Title = "Exercise 1",
            Id = 1,
            Solved = 10,
            Attempted = 20,
            UserIds = new[] { 101, 102, 103 },
            Names = new[] { "Alice", "Bob", "Charlie" }
        },
        new GetExercisesInSessionResponseDto
        {
            Title = "Exercise 2",
            Id = 2,
            Solved = 5,
            Attempted = 10,
            UserIds = new[] { 201, 202 },
            Names = new[] { "Dave", "Eve" }
        }
    };
    }
}