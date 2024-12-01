using API;
using Core.Exercises.Contracts;
using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Shared;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using FluentResults;
using IntegrationTest.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class ExerciseEndpointsTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly ILogger<IMozartService> mozartlLoggerSub = Substitute.For<ILogger<IMozartService>>();

    public ExerciseEndpointsTest(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateExercise_ShouldReturn_201()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub!.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub!.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PostAsJsonAsync("/v1/exercises", requestBody);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_InternalErrorAtSolutionRunner_ShouldReturn_500()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var solutionRunnerResponse = Result.Fail("SolutionRunner sent internal error");
        mozartServiceSub!.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub!.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PostAsJsonAsync("/v1/exercises", requestBody);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_FailedToInsertIntoDatabase_ShouldReturn_500()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub!.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub!.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Fail("Failed to insert into database"));

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PostAsJsonAsync("/v1/exercises", requestBody);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_SolutionRunnerFailureResponse_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var solutionRunnerResponse = new SolutionRunnerResponse
        {
            Action = ResponseCode.Failure,
            ResponseBody = "{}"
        };
        mozartServiceSub!.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub!.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PostAsJsonAsync("/v1/exercises", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_SolutionRunnerErrorResponse_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var solutionRunnerResponse = new SolutionRunnerResponse
        {
            Action = ResponseCode.Error,
            ResponseBody = "{}"
        }; 
        mozartServiceSub!.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub!.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PostAsJsonAsync("/v1/exercises", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_NoAuthentication_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub!.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub!.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PostAsJsonAsync("/v1/exercises", requestBody);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetExercisesByAuthor_ShouldReturn_GetExercisesResponseDtoList()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var exerciseRepoResponse = new List<GetExercisesResponseDto> { new GetExercisesResponseDto(1, "Add numbers")};
        exerciseRepoSub!.GetExercisesAsync(Arg.Any<int>()).Returns(exerciseRepoResponse);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v1/exercises");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<List<GetExercisesResponseDto>>();
        Assert.IsType<List<GetExercisesResponseDto>>(responseContent);
        Assert.Equal(responseContent!.First(), exerciseRepoResponse.First());
    }

    [Fact]
    public async Task GetExercisesByAuthor_NoExercisesFound_ShouldReturn_404()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var exerciseRepoResponse = new List<GetExercisesResponseDto> { };
        exerciseRepoSub!.GetExercisesAsync(Arg.Any<int>()).Returns(exerciseRepoResponse);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.GetAsync("/v1/exercises");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetExercisesByAuthor_NoAuthentication_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        var exerciseRepoResponse = new List<GetExercisesResponseDto> { new GetExercisesResponseDto(1, "Add numbers") };
        exerciseRepoSub!.GetExercisesAsync(Arg.Any<int>()).Returns(exerciseRepoResponse);

        var response = await _client.GetAsync("/v1/exercises");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetExerciseById_ShouldReturn_GetExerciseResponseDto()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();
        
        var exerciseRepoResponse = new GetExerciseResponseDto { Id = 1, Title = "Numbers sum"};
        exerciseRepoSub!.GetExerciseByIdAsync(Arg.Any<int>()).Returns(exerciseRepoResponse);
        var solutionRepoResponse = new List<Testcase> { new Testcase { TestCaseId = 1,  IsPublicVisible = true, Input = { new TestParameter { ParameterType = "int", ParameterValue = "1" } }, Output = { new TestParameter { ParameterType = "int", ParameterValue = "1" } } } };
        solutionRepoSub!.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(solutionRepoResponse);

        var response = await _client.GetAsync("/v1/exercises/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<GetExerciseResponseDto>();
        Assert.IsType<GetExerciseResponseDto>(responseContent);
        Assert.Equal(exerciseRepoResponse.Title, responseContent.Title);
        Assert.Equal(solutionRepoResponse.First().Input.First().ParameterValue, responseContent.TestCases.First().InputParams.First());
    }

    [Fact]
    public async Task GetExerciseById_NoExerciseFound_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();

        exerciseRepoSub!.GetExerciseByIdAsync(Arg.Any<int>()).Returns((GetExerciseResponseDto?)null);
        var solutionRepoResponse = new List<Testcase> { new Testcase { TestCaseId = 1, IsPublicVisible = true, Input = { new TestParameter { ParameterType = "int", ParameterValue = "1" } }, Output = { new TestParameter { ParameterType = "int", ParameterValue = "1" } } } };
        solutionRepoSub!.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(solutionRepoResponse);

        var response = await _client.GetAsync("/v1/exercises/1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExerciseById_FoundNoTestcases_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();

        var exerciseRepoResponse = new GetExerciseResponseDto { Id = 1, Title = "Numbers sum" };
        exerciseRepoSub!.GetExerciseByIdAsync(Arg.Any<int>()).Returns(exerciseRepoResponse);
        var solutionRepoResponse = new List<Testcase> { };
        solutionRepoSub!.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(solutionRepoResponse);

        var response = await _client.GetAsync("/v1/exercises/1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturn_200()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PutAsJsonAsync("v1/exercises/1", requestBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExercise_RepositoryError_ShouldReturn_500()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Fail("Update of database failed"));

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PutAsJsonAsync("v1/exercises/1", requestBody);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExercise_FailedToValidateSolution_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var solutionRunnerResponse = new SolutionRunnerResponse
        {
            Action = ResponseCode.Failure,
        };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PutAsJsonAsync("v1/exercises/1", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExercise_CompilationErrorOnSolutionRunner_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var solutionRunnerResponse = new SolutionRunnerResponse
        {
            Action = ResponseCode.Error,
        };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PutAsJsonAsync("v1/exercises/1", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExercise_NoAuthentication_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        exerciseRepoSub.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var requestBody = CreateExerciseRequestBody();

        var response = await _client.PutAsJsonAsync("v1/exercises/1", requestBody);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturn_204()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        exerciseRepoSub.DeleteExerciseAsync(Arg.Any<int>()).Returns(true);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v1/exercises/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_RepositoryFailedToDelete_ShouldReturn_404()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        exerciseRepoSub.DeleteExerciseAsync(Arg.Any<int>()).Returns(false);

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);

        var response = await _client.DeleteAsync("/v1/exercises/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_NoAuthentication_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();

        exerciseRepoSub.VerifyExerciseAuthorAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        exerciseRepoSub.DeleteExerciseAsync(Arg.Any<int>()).Returns(true);

        var response = await _client.DeleteAsync("/v1/exercises/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSolutionProposal_ShouldReturn_200()
    {
        using var scope = _factory.Services.CreateScope();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();

        solutionRepoSub.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 1 };
        solutionRepoSub.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        var testcasesResponse = new List<Testcase> { new Testcase { TestCaseId = 1, IsPublicVisible = true, Input = { new TestParameter { ParameterType = "int", ParameterValue = "1" } }, Output = { new TestParameter { ParameterType = "int", ParameterValue = "1" } } } };
        solutionRepoSub.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(testcasesResponse);
        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        solutionRepoSub.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var userId = 1;
        var roles = new List<Roles> { Roles.AnonymousUser };
        _client.AddRoleAuth(userId, roles);
        var requestBody = new SubmitSolutionDto(1, "x + y", 1);

        var response = await _client.PostAsJsonAsync("/v2/exercises/1/submission", requestBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task SubmitSolutionProposalPython_ShouldReturn_200()
    {
        using var scope = _factory.Services.CreateScope();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();

        solutionRepoSub.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 2 };
        solutionRepoSub.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        var testcasesResponse = new List<Testcase> { new Testcase { TestCaseId = 1, IsPublicVisible = true, Input = { new TestParameter { ParameterType = "int", ParameterValue = "1" } }, Output = { new TestParameter { ParameterType = "int", ParameterValue = "1" } } } };
        solutionRepoSub.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(testcasesResponse);
        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        solutionRepoSub.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var userId = 1;
        var roles = new List<Roles> { Roles.AnonymousUser };
        _client.AddRoleAuth(userId, roles);
        var requestBody = new SubmitSolutionDto(1, "x + y", 2);

        var response = await _client.PostAsJsonAsync("/v2/exercises/1/submission", requestBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSolutionProposal_SolutionProposalFailed_ShouldReturn_400()
    {
        using var scope = _factory.Services.CreateScope();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();

        solutionRepoSub.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 1 };
        solutionRepoSub.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        var testcasesResponse = new List<Testcase> { new Testcase { TestCaseId = 1, IsPublicVisible = true, Input = { new TestParameter { ParameterType = "int", ParameterValue = "1" } }, Output = { new TestParameter { ParameterType = "int", ParameterValue = "1" } } } };
        solutionRepoSub.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(testcasesResponse);
        var solutionRunnerResponse = new SolutionRunnerResponse
        {
            Action = ResponseCode.Failure,
        };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        solutionRepoSub.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var userId = 1;
        var roles = new List<Roles> { Roles.AnonymousUser };
        _client.AddRoleAuth(userId, roles);
        var requestBody = new SubmitSolutionDto(1, "x + y", 1);

        var response = await _client.PostAsJsonAsync("/v2/exercises/1/submission", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSolutionProposal_UserNotAssociatedToSession_ShouldReturn_500()
    {
        using var scope = _factory.Services.CreateScope();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();

        solutionRepoSub.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        var testcasesResponse = new List<Testcase> { new Testcase { TestCaseId = 1, IsPublicVisible = true, Input = { new TestParameter { ParameterType = "int", ParameterValue = "1" } }, Output = { new TestParameter { ParameterType = "int", ParameterValue = "1" } } } };
        solutionRepoSub.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(testcasesResponse);
        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        solutionRepoSub.InsertSolvedRelation(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);

        var userId = 1;
        var roles = new List<Roles> { Roles.AnonymousUser };
        _client.AddRoleAuth(userId, roles);
        var requestBody = new SubmitSolutionDto(1, "x + y", 1);

        var response = await _client.PostAsJsonAsync("/v2/exercises/1/submission", requestBody);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSolutionProposal_NoAuthorization_ShouldReturn_401()
    {
        using var scope = _factory.Services.CreateScope();
        var solutionRepoSub = scope.ServiceProvider.GetService<ISolutionRepository>();
        var mozartServiceSub = scope.ServiceProvider.GetService<IMozartService>();

        solutionRepoSub.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var testcasesResponse = new List<Testcase> { new Testcase { TestCaseId = 1, IsPublicVisible = true, Input = { new TestParameter { ParameterType = "int", ParameterValue = "1" } }, Output = { new TestParameter { ParameterType = "int", ParameterValue = "1" } } } };
        solutionRepoSub.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(testcasesResponse);
        var solutionRunnerResponse = new SolutionRunnerResponse { Action = ResponseCode.Pass };
        mozartServiceSub.SubmitSubmission(Arg.Any<SubmissionDto>(), Arg.Any<Language>()).Returns(solutionRunnerResponse);
        solutionRepoSub.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var requestBody = new SubmitSolutionDto(1, "x + y", 1);

        var response = await _client.PostAsJsonAsync("/v2/exercises/1/submission", requestBody);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private object CreateExerciseRequestBody()
    {
        return new
        {
            Name = "Sum of Two Numbers",
            Description = "A function that calculates the sum of two integers.",
            Solution = "solution x =\n  if x < 0\n     x * (-1)\n    else x",
            InputParameterType = new[] { "int" },
            OutputParamaterType = new[] { "int" },
            Testcases = new[]
            {
                new { InputParams = new[] { "1" }, OutputParams = new[] { "1" }, PublicVisible = true },
                new { InputParams = new[] { "-1" }, OutputParams = new[] { "1" }, PublicVisible = false }
            }
        };
    }

}
