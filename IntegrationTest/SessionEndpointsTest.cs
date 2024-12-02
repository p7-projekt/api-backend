using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using API;
using Core.Classrooms.Contracts;
using Core.Exercises.Models;
using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using FluentResults;
using Infrastructure.Authentication.Models;
using IntegrationTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class SessionEndpointsTest: IClassFixture<TestWebApplicationFactory<Program>>
{
	private readonly HttpClient _client;
	private readonly TestWebApplicationFactory<Program> _factory;

	public SessionEndpointsTest(TestWebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task GetAuthorSession_ShouldReturn_ListOfSessions()
	{
        using var scope = _factory.Services.CreateScope();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
		sessionSub!.GetInstructorSessionsAsync(Arg.Any<int>()).Returns(new List<Session>{new Session{Title = "Hello"}});
		var userId = 1;
		var roles = new List<Roles> { Roles.Instructor};
		_client.AddRoleAuth(userId, roles);
		
		var response = await _client.GetAsync("/v1/sessions");
		
		Assert.True(response.IsSuccessStatusCode);
		var body = await response.Content.ReadFromJsonAsync<List<GetSessionResponseDto>>();
		Assert.Single(body!);
		Assert.Equal("Hello", body!.First().Title);
	}
	
	[Fact]
	public async Task GetStudentSession_ShouldReturn_ListOfSessions()
	{
		using var scope = _factory.Services.CreateScope();
		var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
		sessionSub!.GetStudentSessionsAsync(Arg.Any<int>()).Returns(new List<Session>{new Session{Title = "Hello"}});
		var userId = 1;
		var roles = new List<Roles> { Roles.Student};
		_client.AddRoleAuth(userId, roles);
    	
		var response = await _client.GetAsync("/v1/sessions");
    	
		Assert.Equal(HttpStatusCode.OK
			,response.StatusCode);
	}
	
	[Fact]
    public async Task GetAuthorSession_ShouldReturn_FailedResponse()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
    	sessionSub!.GetInstructorSessionsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Session>?>(null));
    	var userId = 1;
    	var roles = new List<Roles> { Roles.Instructor};
    	_client.AddRoleAuth(userId, roles);
    	
    	var response = await _client.GetAsync("/v1/sessions");
    	
    	Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
    }
	[Fact]
    public async Task GetAuthorSession_ShouldReturn_FailedAuthentication()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
    	sessionSub!.GetInstructorSessionsAsync(Arg.Any<int>()).Returns(new List<Session>{new Session{Title = "Hello"}}); 
    	var userId = 1;
    	var roles = new List<Roles> { Roles.AnonymousUser};
    	_client.AddRoleAuth(userId, roles);
    	
    	var response = await _client.GetAsync("/v1/sessions");
    	
    	Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
    }

    [Fact]
    public async Task DeleteSession_ShouldReturn_204()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.DeleteSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v1/sessions/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSession_RepositoryFailedToDelete_ShouldReturn_404()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.DeleteSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v1/sessions/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSession_NoAuthorization_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.DeleteSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);

        var response = await _client.DeleteAsync("/v1/sessions/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(Roles.Instructor)]
    [InlineData(Roles.AnonymousUser)]
    public async Task GetExeciseById_ValidRoles_ShouldReturn_GetSessionResponseDto(Roles role)
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        sessionRepoSub.VerifyParticipantAccess(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var sessionResponse = CreateSessionResponse();
        sessionRepoSub.GetSessionOverviewAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(sessionResponse);

        var userId = 1;
        var roles = new List<Roles> { role };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v1/sessions/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); 
        var body = await response.Content.ReadFromJsonAsync<GetSessionResponseDto>();
        Assert.IsType<GetSessionResponseDto>(body);
        Assert.Equal(sessionResponse.Title, body.Title);
    }

    [Theory]
    [InlineData(Roles.Instructor)]
    [InlineData(Roles.AnonymousUser)]
    public async Task GetExeciseById_RoleVerificationFails_ShouldReturn_404(Roles role)
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        sessionRepoSub.VerifyParticipantAccess(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        var sessionResponse = CreateSessionResponse();
        sessionRepoSub.GetSessionOverviewAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(sessionResponse);

        var userId = 1;
        var roles = new List<Roles> { role };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v1/sessions/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetExeciseById_NoAuthentication_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        sessionRepoSub.VerifyParticipantAccess(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var sessionResponse = CreateSessionResponse();
        sessionRepoSub.GetSessionOverviewAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(sessionResponse);

        var response = await _client.GetAsync("/v1/sessions/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSession_ShouldReturn_SessionCode()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns(1);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateSessionDtoInput();

        var response = await _client.PostAsJsonAsync("/v1/sessions", requestBody);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateSessionResponseDto>();
        Assert.IsType<CreateSessionResponseDto>(body);
        Assert.NotNull(body.SessionCode);
    }

    [Fact]
    public async Task CreateSession_NoLanguagesFound_ShouldReturn_400()
    {
	    using var scope = _factory.Services.CreateScope();
	    var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

	    sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns((int)SessionService.ErrorCodes.LanguagesDoesNotExist);

	    var userId = 1;
	    var roles = new List<Roles> { Roles.Instructor };
	    _client.AddRoleAuth(userId, roles);
	    var requestBody = CreateSessionDtoInput();

	    var response = await _client.PostAsJsonAsync("/v1/sessions", requestBody);

	    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateSession_NoExercisesFound_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns(-1);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateSessionDtoInput();

        var response = await _client.PostAsJsonAsync("/v1/sessions", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSession_NoAuthorization_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();

        sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns(1);

        var requestBody = CreateSessionDtoInput();

        var response = await _client.PostAsJsonAsync("/v1/sessions", requestBody);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JoinSession_ShouldReturn_JoinSessionResponseDto()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();
        var sessionResponse = CreateSessionResponse();

        sessionRepoSub.GetSessionBySessionCodeAsync(Arg.Any<string>()).Returns(sessionResponse);
        sessionRepoSub.CreateAnonUser(Arg.Any<string>(), Arg.Any<int>()).Returns(1);

        var requestBody = new JoinDto("AA1234", "lars");

        var response = await _client.PostAsJsonAsync("/join", requestBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JoinResponseDto>();
        Assert.IsType<JoinResponseDto>(body);
    }

    [Fact]
    public async Task JoinSession_FailedToFindSession_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();
        var sessionResponse = CreateSessionResponse();

        sessionRepoSub.GetSessionBySessionCodeAsync(Arg.Any<string>()).Returns(Result.Fail("Found no session on session code"));
        sessionRepoSub.CreateAnonUser(Arg.Any<string>(), Arg.Any<int>()).Returns(1);

        var requestBody = new JoinDto("AA1234", "lars");

        var response = await _client.PostAsJsonAsync("/join", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Session CreateSessionResponse()
    {
        return new Session
        {
            Id = 1,
            Title = "Sum Numbers",
            Description = "A training session focused on basic concepts.",
            AuthorId = 101,
            AuthorName = "John Doe",
            ExpirationTimeUtc = DateTime.UtcNow.AddHours(7),
            SessionCode = "AA1234",
            Exercises = new List<int> { 1, },
            ExerciseDetails = new List<SolvedExercise>
            {
                new SolvedExercise
                {
                    ExerciseId = 1,
                    ExerciseTitle = "Introduction to C#",
                    Solved = true
                }
            }       
        };
    }

    private CreateSessionDto CreateSessionDtoInput()
    {
        return new CreateSessionDto(
            Title: "Number sum",
            Description: "Basic exercise",
            ExpiresInHours: 5,
            ExerciseIds: new List<int> { 101 },
            LanguageIds: new List<int> { 1 }
        );
    }

    [Theory]
    [InlineData("1A")]
    [InlineData("A1234B")]
    [InlineData("1AAGD2")]
    [InlineData("")]
    [InlineData("AB12341")]
    public async Task JoinStudent_ShouldReturn_BadRequestWrongCode(string code)
    {
	    using var scope = _factory.Services.CreateScope();
	    _client.AddRoleAuth(1, new List<Roles>(){Roles.Student});
	    var dto = new JoinDto(code, null);
	    
	    var response = await _client.PostAsJsonAsync("/join", dto);
	    
	    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task JoinStudent_ShouldReturn_JoinSessionFailed()
    {
	    using var scope = _factory.Services.CreateScope();
	    var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();
	    sessionRepoSub.StudentJoinSession(Arg.Any<string>(), Arg.Any<int>()).Returns(Result.Fail("Failed to join student"));
	    _client.AddRoleAuth(1, new List<Roles>(){Roles.Student});
	    var dto = new JoinDto("AV1234", null);
	    
	    var response = await _client.PostAsJsonAsync("/join", dto);
	    
	    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task JoinStudent_ShouldReturn_JoinSession()
    {
	    using var scope = _factory.Services.CreateScope();
	    var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();
	    sessionRepoSub.StudentJoinSession(Arg.Any<string>(), Arg.Any<int>()).Returns(1);
	    _client.AddRoleAuth(1, new List<Roles>(){Roles.Student});
	    var dto = new JoinDto("AV1234", null);
	    
	    var response = await _client.PostAsJsonAsync("/join", dto);
	    
	    Assert.True(response.IsSuccessStatusCode);
	    var obj = await response.Content.ReadFromJsonAsync<JoinResponseDto>();
	    Assert.Equal(JoinedType.TimedSession, obj!.JoinedType);
	    Assert.Equal(1, obj.JoinedId);
    }
    
    [Fact]
    public async Task JoinStudent_ShouldReturn_JoinClassroomFailed()
    {
	    using var scope = _factory.Services.CreateScope();
	    var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();
	    classroomRepoSub.JoinClassroomAsync( Arg.Any<int>(), Arg.Any<string>()).Returns(Result.Fail("Failed to join classroom"));
	    _client.AddRoleAuth(1, new List<Roles>(){Roles.Student});
	    var dto = new JoinDto("1234AV", null);
	    
	    var response = await _client.PostAsJsonAsync("/join", dto);
	    
	    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task JoinStudent_ShouldReturn_JoinClassroom()
    {
	    using var scope = _factory.Services.CreateScope();
	    var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();
	    classroomRepoSub.JoinClassroomAsync( Arg.Any<int>(), Arg.Any<string>()).Returns(1);
	    _client.AddRoleAuth(1, new List<Roles>(){Roles.Student});
	    var dto = new JoinDto("1234AV", null);
	    
	    var response = await _client.PostAsJsonAsync("/join", dto);
	    
	    Assert.True(response.IsSuccessStatusCode);
	    var obj = await response.Content.ReadFromJsonAsync<JoinResponseDto>();
	    Assert.Equal(JoinedType.Classroom, obj!.JoinedType);
	    Assert.Equal(1, obj.JoinedId);
    }
}