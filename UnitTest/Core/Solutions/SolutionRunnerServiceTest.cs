using System.Net;
using System.Text;
using System.Text.Json;
using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Solutions;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Core.Solutions.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitTest.Core.Exercises;

namespace UnitTest.Core.Solutions;

public class SolutionRunnerServiceTest
{
    private readonly ILogger<SolutionRunnerService> loggerSub = Substitute.For<ILogger<SolutionRunnerService>>();
    private readonly ILogger<MozartService> mozartLoggerSub = Substitute.For<ILogger<MozartService>>();

    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_FailUserNotInSession()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"error\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);

        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_FailNoLanguages()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);

        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(null));

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_FailNoTestCases()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);

        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(null));
        solutionRepo.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(new LanguageSupport());

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_FailSubmitSolutionFailed()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);

        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 1 };
        solutionRepo.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));
        solutionRepo.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_FailSubmitSolutionResultFailure()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"failure\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);

        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 1 };
        solutionRepo.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));
        solutionRepo.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value != null);
    }

    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_FailSubmitSolutionResultError()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"error\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);
        
        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 1 };
        solutionRepo.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));
        solutionRepo.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value != null);
    }

    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_FailCoultNotInsertSolved()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);

        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 1 };
        solutionRepo.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));
        solutionRepo.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(false);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task SubmitSolutionAsync_ShouldReturn_Ok()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var mozartService = new MozartService(client, mozartLoggerSub);
        var runner = new SolutionRunnerService(loggerSub, solutionRepo, mozartService);
        var dto = new SubmitSolutionDto(1, "test", 1);

        solutionRepo.CheckUserAssociationToSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var language = new LanguageSupport { Id = 1 };
        solutionRepo.GetSolutionLanguageBySession(Arg.Any<int>(), Arg.Any<int>()).Returns(language);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));
        solutionRepo.InsertSubmissionRelation(Arg.Any<Submission>()).Returns(true);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsSuccess);
    }
}