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
    public async void GetExercisesInSession_Timed_ShouldReturn_GetExercisesCombinedInfoDto()
    {
        // Arrange
        var loggerSub2 = Substitute.For<ILogger<DashboardService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var dashboardRepoSub = Substitute.For<IDashboardRepository>();
        var dashboardService = new DashboardService(loggerSub2, dashboardRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardRepoSub.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(false);
        dashboardRepoSub.GetConnectedTimedUsersAsync(Arg.Any<int>()).Returns(4);
        dashboardRepoSub.GetExercisesInTimedSessionBySessionIdAsync(Arg.Any<int>()).Returns(createExerciseResponse());
        // Act
        var result = await dashboardService.GetExercisesInSession(1, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<GetExercisesInSessionCombinedInfo>(result.Value);
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
    private GetExercisesInSessionCombinedInfo CreateDummyCombinedInfo()
    {
        return new GetExercisesInSessionCombinedInfo(
            Participants: 4,
            ExerciseDetails: new List<GetExercisesAndUserDetailsInSessionResponseDto>
            {
            new GetExercisesAndUserDetailsInSessionResponseDto(
                Title: "Exercise 1",
                Id: 1,
                Solved: 10,
                Attemped: 20,
                UserDetails: new List<UserDetailDto>
                {
                    new UserDetailDto(Id: 101, Name: "Alice"),
                    new UserDetailDto(Id: 102, Name: "Bob"),
                    new UserDetailDto(Id: 103, Name: "Charlie")
                }
            ),
            new GetExercisesAndUserDetailsInSessionResponseDto(
                Title: "Exercise 2",
                Id: 2,
                Solved: 5,
                Attemped: 10,
                UserDetails: new List<UserDetailDto>
                {
                    new UserDetailDto(Id: 201, Name: "Dave"),
                    new UserDetailDto(Id: 202, Name: "Eve")
                }
            )
            }
        );
    }
}