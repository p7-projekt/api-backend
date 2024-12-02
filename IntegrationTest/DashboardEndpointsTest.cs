using IntegrationTest.Setup;
using API;
using Core.Exercises.Contracts;
using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Shared;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Net.Http.Json;
using System.Net;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Dashboards.Contracts;
using Core.Dashboards.Models;
using System.Text.Json;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class DashboardEndpointsTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;

    public DashboardEndpointsTest(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetExercisesInSession_ShouldReturn_FailedAuthentication()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        var returnValue = createExerciseResponse();
        dashboardSub!.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(false);
        dashboardSub!.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(returnValue);
        dashboardSub!.GetConnectedTimedUsersAsync(Arg.Any<int>()).Returns(21);

        var userId = 1;
        var roles = new List<Roles> { Roles.AnonymousUser };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetExercisesInSession_ShouldReturn_NotFound()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetExercisesInSession_ShouldReturn_OK_Classroom_Exercises()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var returnValue = createExerciseResponse();
        dashboardSub!.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(true);
        dashboardSub!.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(returnValue);
        dashboardSub!.GetConnectedUsersClassAsync(Arg.Any<int>()).Returns(4);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact]
    public async Task GetExercisesInSession_ShouldReturn_OK_TimedSession_Exercises()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var returnValue = createExerciseResponse();
        dashboardSub!.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(false);
        dashboardSub!.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(returnValue);
        dashboardSub!.GetConnectedTimedUsersAsync(Arg.Any<int>()).Returns(4);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact]
    public async Task GetExercisesInTimedSession_ShouldReturn_GetExercisesInSessionCombinedInfo()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var returnValue = createExerciseResponse();
        dashboardSub!.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(false);
        dashboardSub!.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(returnValue);
        dashboardSub!.GetConnectedTimedUsersAsync(Arg.Any<int>()).Returns(4);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetExercisesInSessionCombinedInfo>();
        Assert.IsType<GetExercisesInSessionCombinedInfo>(responseContent);
        var expectedJson = JsonSerializer.Serialize(CreateDummyCombinedInfo());
        var actualJson = JsonSerializer.Serialize(responseContent);
        Assert.Equal(actualJson, expectedJson);
    }
    [Fact]
    public async Task GetExercisesInTimedSession_ShouldFail_GetExercisesInSessionCombinedInfo()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var returnValue = new List<GetExercisesInSessionResponseDto>
        {
        new GetExercisesInSessionResponseDto
        {
            Title = "Exercise 1",
            Id = 1,
            Solved = 10,
            Attempted = 20,
            UserIds = new[] { 101, 102, 103 },
            Names = new[] { "Alice", "Bob", "Charliee" }
        },
        new GetExercisesInSessionResponseDto
        {
            Title = "Exercise 2",
            Id = 2,
            Solved = 5,
            Attempted = 10,
            UserIds = new[] { 201, 202 },
            Names = new[] { "Dave", "Eve" }
        } };
        dashboardSub!.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(false);
        dashboardSub!.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(returnValue);
        dashboardSub!.GetConnectedTimedUsersAsync(Arg.Any<int>()).Returns(4);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetExercisesInSessionCombinedInfo>();
        Assert.IsType<GetExercisesInSessionCombinedInfo>(responseContent);
        var expectedJson = JsonSerializer.Serialize(CreateDummyCombinedInfo());
        var actualJson = JsonSerializer.Serialize(responseContent);
        Assert.NotEqual(actualJson, expectedJson);
    }
    [Fact]
    public async Task GetExercisesInClassSession_ShouldReturn_GetExercisesInSessionCombinedInfo()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var returnValue = createExerciseResponse();
        dashboardSub!.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(true);
        dashboardSub!.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(returnValue);
        dashboardSub!.GetConnectedUsersClassAsync(Arg.Any<int>()).Returns(4);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetExercisesInSessionCombinedInfo>();
        Assert.IsType<GetExercisesInSessionCombinedInfo>(responseContent);
        var expectedJson = JsonSerializer.Serialize(CreateDummyCombinedInfo());
        var actualJson = JsonSerializer.Serialize(responseContent);
        Assert.Equal(actualJson, expectedJson);
    }
    [Fact]
    public async Task GetExercisesInClassSession_ShouldFail_GetExercisesInSessionCombinedInfo()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        sessionSub!.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var returnValue = new List<GetExercisesInSessionResponseDto>
        {
        new GetExercisesInSessionResponseDto
        {
            Title = "Exercise 1",
            Id = 1,
            Solved = 10,
            Attempted = 20,
            UserIds = new[] { 101, 102, 103 },
            Names = new[] { "Alice", "Bob", "Charlddiee" }
        },
        new GetExercisesInSessionResponseDto
        {
            Title = "Exercise 2",
            Id = 2,
            Solved = 5,
            Attempted = 10,
            UserIds = new[] { 201, 202 },
            Names = new[] { "Dave", "Eve" }
        } };
        dashboardSub!.CheckSessionInClassroomAsync(Arg.Any<int>()).Returns(true);
        dashboardSub!.GetExercisesInSessionBySessionIdAsync(Arg.Any<int>()).Returns(returnValue);
        dashboardSub!.GetConnectedUsersClassAsync(Arg.Any<int>()).Returns(4);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetExercisesInSessionCombinedInfo>();
        Assert.IsType<GetExercisesInSessionCombinedInfo>(responseContent);
        var expectedJson = JsonSerializer.Serialize(CreateDummyCombinedInfo());
        var actualJson = JsonSerializer.Serialize(responseContent);
        Assert.NotEqual(actualJson, expectedJson);
    }
    [Fact]
    public async Task GetUserSolution_ShouldReturn_FailedAuthentication()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();

        var userId = 1;
        var roles = new List<Roles> { Roles.AnonymousUser };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [Fact]
    public async Task GetUserSolution_NotAuthor_ShouldReturn_FailedAuthentication()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        dashboardSub!.CheckAuthorizedToGetSolution(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/solution/1/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [Fact]
    public async Task GetUserSolution_InvalidId_ShouldReturn_NotFound()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        dashboardSub!.CheckAuthorizedToGetSolution(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        dashboardSub!.GetSolutionByUserIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Fail("Not found"));

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/solution/1/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task GetUserSolution_ShouldReturn_GetExerciseSolutionResponseDto()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        dashboardSub!.CheckAuthorizedToGetSolution(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var solutionResponse = CreateDummySolutionData();
        dashboardSub!.GetSolutionByUserIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Ok(solutionResponse));


        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/solution/1/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetExerciseSolutionResponseDto>();
        Assert.IsType<GetExerciseSolutionResponseDto>(responseContent);
        var expectedJson = JsonSerializer.Serialize(CreateDummySolutionData());
        var actualJson = JsonSerializer.Serialize(responseContent);
        Assert.Equal(actualJson, expectedJson);
    }
    [Fact]
    public async Task GetUserSolution_ShouldFail_Return_GetExerciseSolutionResponseDto()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardSub = scope.ServiceProvider.GetService<IDashboardRepository>();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
        dashboardSub!.CheckAuthorizedToGetSolution(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var solutionResponse = new GetExerciseSolutionResponseDto(
            Title: "Sample Exercise",
            Description: "This is a sample exercise description.",
            Solution: "def solution():\n    return 'Hello, Woooooooooooooorld!'",
            Language: "Python",
            Language_id: 1
        );
        dashboardSub!.GetSolutionByUserIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Ok(solutionResponse));


        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/dashboard/solution/1/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetExerciseSolutionResponseDto>();
        Assert.IsType<GetExerciseSolutionResponseDto>(responseContent);
        var expectedJson = JsonSerializer.Serialize(CreateDummySolutionData());
        var actualJson = JsonSerializer.Serialize(responseContent);
        Assert.NotEqual(actualJson, expectedJson);
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
    private GetExerciseSolutionResponseDto CreateDummySolutionData()
    {
        return new GetExerciseSolutionResponseDto(
            Title: "Sample Exercise",
            Description: "This is a sample exercise description.",
            Solution: "def solution():\n    return 'Hello, World!'",
            Language: "Python",
            Language_id: 1
        );
    }
}
