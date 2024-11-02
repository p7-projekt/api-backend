using System.Net;
using System.Net.Http.Json;
using System.Text;
using Core.Exercises.Models;
using Core.Solutions.Models;
using Core.Solutions.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Solutions;


public class HaskellServiceTest
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _responseMessage;

        public MockHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseMessage);
        }
    }
    
    [Fact]
    public void SubmitSolution_ShouldReturn_ExceptionMissedEnvironmentVariable()
    {
        var httpClientSub = Substitute.For<HttpClient>();
        var loggerSub = Substitute.For<ILogger<HaskellService>>();
        Assert.Throws<NullReferenceException>(() => new HaskellService(httpClientSub, loggerSub));
    }

    [Fact]
    public async Task SubmitSolution_ShouldReturn_FailUnprocessableContent()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.UnprocessableContent
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var loggerSub = Substitute.For<ILogger<HaskellService>>();
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var haskellService = new HaskellService(client, loggerSub);
        var dto = SubmissionMapper.ToSubmission(
            new List<Testcase>
            {
                new Testcase()
            }, "hello");
        var result = await haskellService.SubmitSubmission(dto);
        
        Assert.True(result.IsFailed);
    }
    [Fact]
    public async Task SubmitSolution_ShouldReturn_FailInternalServerError()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var loggerSub = Substitute.For<ILogger<HaskellService>>();
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var haskellService = new HaskellService(client, loggerSub);
        var dto = SubmissionMapper.ToSubmission(
            new List<Testcase>
            {
                new Testcase()
            }, "hello");
        var result = await haskellService.SubmitSubmission(dto);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task SubmitSolution_ShouldReturn_FailException()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"unknown\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var loggerSub = Substitute.For<ILogger<HaskellService>>();
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var haskellService = new HaskellService(client, loggerSub);
        var dto = SubmissionMapper.ToSubmission(
            new List<Testcase>
            {
                new Testcase()
            }, "hello");
        await Assert.ThrowsAsync<Exception>(async () => await haskellService.SubmitSubmission(dto));
        
    }
    
    [Fact]
    public async Task SubmitSolution_ShouldReturn_OkPass()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"pass\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var loggerSub = Substitute.For<ILogger<HaskellService>>();
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var haskellService = new HaskellService(client, loggerSub);
        var dto = SubmissionMapper.ToSubmission(
            new List<Testcase>
            {
                new Testcase()
            }, "hello");
        var result = await haskellService.SubmitSubmission(dto);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(ResponseCode.Pass, result.Value.Action);
    }
    
    [Fact]
    public async Task SubmitSolution_ShouldReturn_OkFailure()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"failure\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var loggerSub = Substitute.For<ILogger<HaskellService>>();
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var haskellService = new HaskellService(client, loggerSub);
        var dto = SubmissionMapper.ToSubmission(
            new List<Testcase>
            {
                new Testcase()
            }, "hello");
        var result = await haskellService.SubmitSubmission(dto);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(ResponseCode.Failure, result.Value.Action);
    }
    
    [Fact]
    public async Task SubmitSolution_ShouldReturn_OkError()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"Result\": \"error\"}", Encoding.UTF8, "application/json")
        };
        var httpClientSub = new MockHttpMessageHandler(response);
        var client = new HttpClient(httpClientSub);
        var loggerSub = Substitute.For<ILogger<HaskellService>>();
        Environment.SetEnvironmentVariable("MOZART_HASKELL", "url");
        var haskellService = new HaskellService(client, loggerSub);
        var dto = SubmissionMapper.ToSubmission(
            new List<Testcase>
            {
                new Testcase()
            }, "hello");
        var result = await haskellService.SubmitSubmission(dto);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(ResponseCode.Error, result.Value.Action);
    }
}