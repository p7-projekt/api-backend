using API;
using Core.Classrooms.Contracts;
using Core.Classrooms.Models;
using Core.Exercises.Models;
using Core.Sessions.Contracts;
using Core.Shared;
using FluentResults;
using IntegrationTest.Setup;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class ClassroomEndpointsTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;

    public ClassroomEndpointsTest(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateClassroom_ShouldReturn_201()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();
        
        classroomRepoSub.InsertClassroomAsync(Arg.Any<ClassroomDto>(), Arg.Any<int>(), Arg.Any<string>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Title = "Valid Title", Description = "" };

        var response = await _client.PostAsJsonAsync("/v2/classrooms", requestBody);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateClassroom_ClassroomRepoFailed_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.InsertClassroomAsync(Arg.Any<ClassroomDto>(), Arg.Any<int>(), Arg.Any<string>()).Returns(Result.Fail("Failed to insert"));

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Title = "Valid Title", Description = "" };

        var response = await _client.PostAsJsonAsync("/v2/classrooms", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddSessionToClassroom_ShouldReturn_201()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.AddSessionToClassroomAsync(Arg.Any<ClassroomSessionDto>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Title = "Valid title", ExerciseIds = new List<int> { 1, 2 }, LanguageIds = new List<int> { 1 } };

        var response = await _client.PostAsJsonAsync("/v2/classrooms/1/session", requestBody);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddSessionToClassroom_FailedAuthorValidation_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Title = "Valid title", ExerciseIds = new List<int> { 1, 2 }, LanguageIds = new List<int> { 1 } };

        var response = await _client.PostAsJsonAsync("/v2/classrooms/1/session", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetClassroom_Student_ShouldReturn_200ClassroomResponse()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var classroomResponse = new GetClassroomResponseDto { Id = 5, Roomcode = "1234AA" };
        classroomRepoSub.GetClassroomByIdAsync(Arg.Any<int>()).Returns(classroomResponse);

        var userId = 1;
        var roles = new List<Roles> { Roles.Student };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/classrooms/5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetClassroomResponseDto>();
        Assert.IsType<GetClassroomResponseDto>(responseContent);
        Assert.Equal(5, responseContent.Id);
        Assert.Null(responseContent.Roomcode); 
    }

    [Fact]
    public async Task GetClassroom_Instructor_ShouldReturn_200ClassroomResponse()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var classroomResponse = new GetClassroomResponseDto { Id = 5, Roomcode = "1234AA" };
        classroomRepoSub.GetClassroomByIdAsync(Arg.Any<int>()).Returns(classroomResponse);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/classrooms/5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetClassroomResponseDto>();
        Assert.IsType<GetClassroomResponseDto>(responseContent);
        Assert.Equal(5, responseContent.Id);
        Assert.NotNull(responseContent.Roomcode);
    }

    [Fact]
    public async Task GetClassroom_VerificationFailed_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Student };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/classrooms/5");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetClassrooms_ShouldReturn_200ListOfClassrooms()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        var class1 = new GetClassroomsResponseDto(1, "Title1", "Description");
        var class2 = new GetClassroomsResponseDto(2, "Large title", "Description");
        var classroomResponse = new List<GetClassroomsResponseDto> { class1, class2 };
        classroomRepoSub.GetStudentClassroomsById(Arg.Any<int>()).Returns(classroomResponse);

        var userId = 1;
        var roles = new List<Roles> { Roles.Student };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/classrooms");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<List<GetClassroomsResponseDto>>();
        Assert.IsType<List<GetClassroomsResponseDto>>(responseContent); 
        Assert.Equal(2, responseContent.Count());
        Assert.Equal(2, responseContent[1].Id);
    }

    [Fact]
    public async Task DeleteClassroom_ShouldReturn_204()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.DeleteClassroomAsync(Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v2/classrooms/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClassroom_FailedToDelete_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.DeleteClassroomAsync(Arg.Any<int>()).Returns(Result.Fail("Failed to delete classroom"));

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v2/classrooms/1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClassroom_ShouldReturn_204()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.UpdateClassroomDetailsAsync(Arg.Any<UpdateClassroomDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Title = "New Title", RegistrationOpen = true};
        var response = await _client.PutAsJsonAsync("/v2/classrooms/1", requestBody);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClassroom_InvalidAuthor_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Title = "New Title", RegistrationOpen = true };
        var response = await _client.PutAsJsonAsync("/v2/classrooms/1", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClassroomSession_ShouldReturn_204()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.UpdateClassroomSessionAsync(Arg.Any<UpdateClassroomSessionDto>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Id = 1, Title = "Title", Active = false, ExerciseIds = new List<int> { 1, 2}, LanguageIds = new List<int> { 1 } };
        var response = await _client.PutAsJsonAsync("/v2/classrooms/1/session", requestBody);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClassroomSession_InvalidAuthor_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Id = 1, Title = "Title", Active = false, ExerciseIds = new List<int> { 1, 2 }, LanguageIds = new List<int> { 1 } };
        var response = await _client.PutAsJsonAsync("/v2/classrooms/1/session", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClassroomSession_ShouldReturn_204()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.DeleteClassroomSessionAsync(Arg.Any<int>()).Returns(Task.CompletedTask);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v2/classrooms/session/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClassroomSession_InvalidAuthor_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionRepoSub = scope.ServiceProvider.GetService<ISessionRepository>();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v2/classrooms/session/1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetClassroomSession_ShouldReturn_200ClassroomSession()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        var classroomResponse = new GetClassroomSessionResponseDto { Id = 5 };
        classroomRepoSub.GetClassroomSessionByIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(classroomResponse);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/classrooms/session/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetClassroomSessionResponseDto>();
        Assert.IsType<GetClassroomSessionResponseDto>(responseContent);
        Assert.Equal(5, responseContent.Id);
    }

    [Fact]
    public async Task LeaveClassroom_ShouldReturn_204()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.LeaveClassroomAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Student };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v2/classrooms/1/leave");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task LeaveClassroom_FailedToLeave_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var classroomRepoSub = scope.ServiceProvider.GetService<IClassroomRepository>();

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.LeaveClassroomAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Fail("Failed to leave classroom"));

        var userId = 1;
        var roles = new List<Roles> { Roles.Student };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v2/classrooms/1/leave");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // The following are cases that could be created for each endpoint as they test correct functionality of middleware,
    // but performed on arbitrary endpoints, they intend to establish presedence for the behavior of all endpoints

    [Fact]
    public async Task InvalidRole_ShouldReturn_403()
    {
        var userId = 1;
        var roles = new List<Roles> { Roles.AnonymousUser };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v2/classrooms/session/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task NoUser_ShouldReturn_401()
    {
        var response = await _client.DeleteAsync("/v2/classrooms/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FailedValidation_ShouldReturn_201()
    {
        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var requestBody = new { Title = 2 };

        var response = await _client.PostAsJsonAsync("/v2/classrooms", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
