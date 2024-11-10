using System.Net;
using System.Text;
using Core.Exercises.Models;
using Core.Solutions;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Core.Solutions.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Core.Solutions;

public class SolutionRunnerServiceTest
{
    private readonly ILogger<SolutionRunnnerService> loggerSub = Substitute.For<ILogger<SolutionRunnnerService>>();
    private readonly ILogger<HaskellService> haskellLoggerSub = Substitute.For<ILogger<HaskellService>>();

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
        var haskellService = new HaskellService(client, haskellLoggerSub);
        var runner = new SolutionRunnnerService(loggerSub, haskellService, solutionRepo);
        var dto = new SubmitSolutionDto(1, "test");

        solutionRepo.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

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
        var haskellService = new HaskellService(client, haskellLoggerSub);
        var runner = new SolutionRunnnerService(loggerSub, haskellService, solutionRepo);
        var dto = new SubmitSolutionDto(1, "test");

        solutionRepo.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(null));

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
        var haskellService = new HaskellService(client, haskellLoggerSub);
        var runner = new SolutionRunnnerService(loggerSub, haskellService, solutionRepo);
        var dto = new SubmitSolutionDto(1, "test");

        solutionRepo.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));

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
        var haskellService = new HaskellService(client, haskellLoggerSub);
        var runner = new SolutionRunnnerService(loggerSub, haskellService, solutionRepo);
        var dto = new SubmitSolutionDto(1, "test");

        solutionRepo.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Message == null);
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
        var haskellService = new HaskellService(client, haskellLoggerSub);
        var runner = new SolutionRunnnerService(loggerSub, haskellService, solutionRepo);
        var dto = new SubmitSolutionDto(1, "test");

        solutionRepo.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TestCaseResults == null);
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
        var haskellService = new HaskellService(client, haskellLoggerSub);
        var runner = new SolutionRunnnerService(loggerSub, haskellService, solutionRepo);
        var dto = new SubmitSolutionDto(1, "test");

        solutionRepo.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));
        solutionRepo.InsertSolvedRelation(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsSuccess);
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
        var haskellService = new HaskellService(client, haskellLoggerSub);
        var runner = new SolutionRunnnerService(loggerSub, haskellService, solutionRepo);
        var dto = new SubmitSolutionDto(1, "test");

        solutionRepo.CheckAnonUserExistsInSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Any<int>()).Returns(Task.FromResult<List<Testcase>?>(new List<Testcase>()));
        solutionRepo.InsertSolvedRelation(Arg.Any<int>(), Arg.Any<int>()).Returns(true);

        var result = await runner.SubmitSolutionAsync(dto, exerciseId: 1, userId: 2);

        Assert.True(result.IsSuccess);
    }
}