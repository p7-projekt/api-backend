using Core.Exercises;
using Core.Exercises.Contracts;
using Core.Exercises.Models;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Core.Solutions.Services;
using FluentResults;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Core.Solutions;

namespace UnitTest.Core.Exercises;

public class ExerciseServiceTest
{
    private readonly ILogger<MozartService> mozartlLoggerSub = Substitute.For<ILogger<MozartService>>();


    [Fact]
    public async Task DeleteExercise_ShouldReturn_Ok()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartlService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartlService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.DeleteExerciseAsync(Arg.Is<int>(x => x > 0)).Returns(true);

        var result = await exerciseService.DeleteExercise(1, 1);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 0)]
    public async Task DeleteExercise_ShouldReturn_Fail(int exerciseId, int userId)
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.DeleteExerciseAsync(Arg.Is<int>(x => x > 0)).Returns(true);

        var result = await exerciseService.DeleteExercise(exerciseId, userId);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetExercise_ShouldReturn_Ok()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        var exercise = new GetExerciseResponseDto();
        exerciseRepo.GetExerciseByIdAsync(Arg.Is<int>(x => x > 0)).Returns(exercise);
        var testcases = new List<Testcase> { new Testcase() };
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Is<int>(x => x > 0)).Returns(testcases);

        var result = await exerciseService.GetExerciseById(1);

        Assert.True(result.IsSuccess);
        Assert.IsType<GetExerciseResponseDto>(result.Value);
    }

    [Fact]
    public async Task GetExercise_InvalidId_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        var exercise = new GetExerciseResponseDto();
        exerciseRepo.GetExerciseByIdAsync(Arg.Is<int>(x => x > 0)).Returns(exercise);
        var testcases = new List<Testcase> { new Testcase() };
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Is<int>(x => x > 0)).Returns(testcases);

        var invalidId = 0;
        var result = await exerciseService.GetExerciseById(invalidId);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetExercise_NoTestcases_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        var exercise = new GetExerciseResponseDto();
        exerciseRepo.GetExerciseByIdAsync(Arg.Is<int>(x => x > 0)).Returns(exercise);
        var testcases = new List<Testcase> { };
        solutionRepo.GetTestCasesByExerciseIdAsync(Arg.Is<int>(x => x > 0)).Returns(testcases);

        var result = await exerciseService.GetExerciseById(1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetExercises_ShouldReturn_Ok()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        var exercise = new GetExercisesResponseDto(1, "title");
        exerciseRepo.GetExercisesAsync(Arg.Is<int>(x => x > 0)).Returns(new List<GetExercisesResponseDto> { exercise });

        var result = await exerciseService.GetExercises(1);

        Assert.True(result.IsSuccess);
        Assert.IsType<List<GetExercisesResponseDto>>(result.Value);
    }

    [Fact]
    public async Task GetExercises_InvalidId_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        var exercise = new GetExercisesResponseDto(1, "title");
        exerciseRepo.GetExercisesAsync(Arg.Is<int>(x => x > 0)).Returns(new List<GetExercisesResponseDto> { exercise });

        var invalidId = 0;
        var result = await exerciseService.GetExercises(invalidId);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturn_Ok()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, [ "int" ], [ "int" ], [ new TestcaseDto(["2"], ["4"], true) ]);
        var result = await exerciseService.UpdateExercise(1, 1, dto);

        Assert.True(result.IsSuccess);
        Assert.IsType<SolutionRunnerResponse>(result.Value);
        Assert.Equal(ResponseCode.Pass, result.Value.Action);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 0)]
    public async Task UpdateExercise_InvalidId_ShouldReturn_Fail(int exerciseId, int authorId)
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.UpdateExercise(exerciseId, authorId, dto);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task UpdateExercise_IncorrectStatusCodeFromSolutionRunner_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.UnprocessableContent,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto([ "2" ], [ "4" ], true)]);
        var result = await exerciseService.UpdateExercise(1, 1, dto);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task UpdateExercise_FailedSolution_ShouldReturn_OkWithFailure()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"failure\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.UpdateExercise(1, 1, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResponseCode.Failure, result.Value.Action);
    }

    [Fact]
    public async Task UpdateExercise_FailedCompilationOrTimeout_ShouldReturn_OkWithError()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"error\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.UpdateExercise(1, 1, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResponseCode.Error, result.Value.Action);
    }

    [Fact]
    public async Task UpdateExercise_ErrorDuringDatabaseUpdateTransaction_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.VerifyExerciseAuthorAsync(Arg.Is<int>(x => x > 0), Arg.Is<int>(x => x > 0)).Returns(true);
        exerciseRepo.UpdateExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Fail("error occured"));

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.UpdateExercise(1, 1, dto);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task CreateExercise_ShouldReturn_Ok()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.CreateExercise(dto, 1);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task CreateExercise_IncorrectStatusCodeFromSolutionRunner_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.UnprocessableContent,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.CreateExercise(dto, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task CreatExercise_FailedSolution_ShouldReturn_OkWithFailure()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"failure\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.CreateExercise(dto, 1);

        Assert.True(result.IsSuccess);
        Assert.IsType<MozartResponseDto>(result.Value);
        Assert.Null(result.Value.Message);
    }

    [Fact]
    public async Task CreateExercise_FailedCompilationOrTimeout_ShouldReturn_OkWithError()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"error\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Ok());

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.CreateExercise(dto, 1);

        Assert.True(result.IsSuccess);
        Assert.IsType<MozartResponseDto>(result.Value);
        Assert.Null(result.Value.TestCaseResults);
    }

    [Fact]
    public async Task CreateExercise_InsertFailed_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var logger = Substitute.For<ILogger<ExerciseService>>();
        var solutionRepo = Substitute.For<ISolutionRepository>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var mozartService = new MozartService(client, mozartlLoggerSub);
        var exerciseService = new ExerciseService(exerciseRepo, logger, solutionRepo, mozartService);

        exerciseRepo.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Is<int>(x => x > 0)).Returns(Result.Fail("Failed insert, transacation rolled back"));

        var dto = new ExerciseDto("title", "description", "2+2", 1, ["int"], ["int"], [new TestcaseDto(["2"], ["4"], true)]);
        var result = await exerciseService.CreateExercise(dto, 1);

        Assert.True(result.IsFailed);
    }
}
