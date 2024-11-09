using API;
using Core.Exercises.Contracts;
using Core.Exercises.Models;
using Core.Sessions.Contracts;
using Core.Shared;
using Core.Solutions.Models;
using Core.Solutions.Services;
using FluentResults;
using IntegrationTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IntegrationTest;

public class ExerciseEndpointsTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly ILogger<HaskellService> haskellLoggerSub = Substitute.For<ILogger<HaskellService>>();

    public ExerciseEndpointsTest(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateExercise_ShouldReturn_201()
    {
        using var scope = _factory.Services.CreateScope();
        var exerciseRepoSub = scope.ServiceProvider.GetService<IExerciseRepository>();
        exerciseRepoSub!.InsertExerciseAsync(Arg.Any<ExerciseDto>(), Arg.Any<int>()).Returns(Result.Ok());
        var haskellServiceSub = scope.ServiceProvider.GetService<IHaskellService>();
        haskellServiceSub!.SubmitSubmission(Arg.Any<SubmissionDto>()).Returns(new SolutionRunnerResponse { Action = ResponseCode.Pass });

        var userId = 1;
        var roles = new List<Roles> { Roles.Instructor };
        _client.AddRoleAuth(userId, roles);
        var requestBody = new
        {
            Name = "Sum of Two Numbers",
            Description = "A function that calculates the sum of two integers.",
            Solution = "solution x =\n  if x < 0\n     x * (-1)\n    else x",
            InputParameterType = new[] { "int", },
            OutputParamaterType = new[] { "int" },
            Testcases = new[]
            {
                new
                {
                    InputParams = new[] { "1", },
                    OutputParams = new[] { "1" },
                    PublicVisible = true
                },
                new
                {
                    InputParams = new[] { "-1", },
                    OutputParams = new[] { "1" },
                    PublicVisible = false
                }
            }
        };
        var jsonBody = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/v1/exercises", jsonBody);

        Assert.True(response.StatusCode == HttpStatusCode.Created);
    }

}
