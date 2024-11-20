using Core.Exercises.Contracts;
using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using Core.Shared.Contracts;
using FluentResults;
using Infrastructure.Authentication;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Core.Sessions;

public class SessionServiceTest
{
    [Fact]
    public void GenerateSessionCode_ShouldReturn_ValidCode()
    {
        // Arrange
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);
        
        // Act
        var result = sessionService.GenerateSessionCode();
        
        // Assert
        Assert.Equal(6, result.Length);
        Assert.Contains(result, x => char.IsLetter(x));
        Assert.Contains(result, x => char.IsDigit(x));
    }

    [Fact]
    public async Task DeleteSession_ShouldReturn_OK()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);

        sessionRepoSub.DeleteSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(true);

        var result =  await sessionService.DeleteSession(1, 1);
        
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task DeleteSession_ShouldReturn_Fail()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);

        sessionRepoSub.DeleteSessionAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var result =  await sessionService.DeleteSession(1, 1);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task GetSessions_ShouldReturn_Sessions()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);

        var session1 = new Session();
        var session2 = new Session();
        sessionRepoSub.GetSessionsAsync(Arg.Any<int>()).Returns(new List<Session> { session1, session2 });

        var result = await sessionService.GetSessions(1);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }
    
    [Fact]
    public async Task GetSessions_ShouldReturn_Fail()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);
        
        sessionRepoSub.GetSessionsAsync(Arg.Any<int>()).Returns(Task.FromResult<IEnumerable<Session>?>(null));

        var result = await sessionService.GetSessions(1);
        
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldReturn_CreateSessionResponseDto()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);

        sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns(1);

        var sessionDto = new CreateSessionDto("Title", null, 4, new List<int>{1,2,3}, new List<int> { 1 });
        var result = await sessionService.CreateSessionAsync(sessionDto, 2);
        
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.SessionCode, char.IsLetter);
    }
    
    [Fact]
    public async Task CreateSessionAsync_ShouldReturn_FailExerciseDoesntExist()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);

        sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns((int)SessionService.ErrorCodes.ExerciseDoesNotExist);

        var sessionDto = new CreateSessionDto("Title", null, 4, new List<int>{1,2,3}, new List<int> { 1 });
        var result = await sessionService.CreateSessionAsync(sessionDto, 2);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task CreateSessionAsync_ShouldReturn_FailUniqueConstraintViolated()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);

        sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns((int)SessionService.ErrorCodes.UniqueConstraintViolation);

        var sessionDto = new CreateSessionDto("Title", null, 4, new List<int>{1,2,3}, new List<int> { 1 });
        var result = await sessionService.CreateSessionAsync(sessionDto, 2);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task CreateSessionAsync_ShouldReturn_OkUniqueConstraintViolated()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenRepoSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenRepoSub);

        sessionRepoSub.InsertSessionAsync(Arg.Any<Session>(), Arg.Any<int>()).Returns((int)SessionService.ErrorCodes.UniqueConstraintViolation, 1);

        var sessionDto = new CreateSessionDto("Title", null, 4, new List<int>{1,2,3}, new List<int> { 1 });
        var result = await sessionService.CreateSessionAsync(sessionDto, 2);
        
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.SessionCode, char.IsLetter);
    }

    [Fact]
    public async Task JoinSessionAnonUser_ShouldReturn_JoinSessionResponseDto()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenServiceSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenServiceSub);

        var session = new Session();
        sessionRepoSub.GetSessionBySessionCodeAsync(Arg.Any<string>()).Returns(session);
        session.ExpirationTimeUtc = DateTime.UtcNow.AddHours(5);

        tokenServiceSub.GenerateAnonymousUserJwt(Arg.Any<int>(), Arg.Any<int>()).Returns("token");

        var dto = new JoinSessionDto("token");
        var result = await sessionService.JoinSessionAnonUser(dto);
        
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task JoinSessionAnonUser_ShouldReturn_Fail()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenServiceSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenServiceSub);

        var session = new Session();
        sessionRepoSub.GetSessionBySessionCodeAsync(Arg.Any<string>()).Returns(Result.Fail("Failed to get session"));
        
        var dto = new JoinSessionDto("token");
        var result = await sessionService.JoinSessionAnonUser(dto);
        
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetSessionByIdAsync_ShouldReturn_FailInstructorAccess()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenServiceSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenServiceSub);
        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        
        var result = await sessionService.GetSessionByIdAsync(1, 1, Roles.Instructor);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task GetSessionByIdAsync_ShouldReturn_FailAnonUserAccess()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenServiceSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenServiceSub);
        sessionRepoSub.VerifyParticipantAccess(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        
        var result = await sessionService.GetSessionByIdAsync(1, 1, Roles.AnonymousUser);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task GetSessionByIdAsync_ShouldReturn_FailNoSession()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenServiceSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenServiceSub);
        sessionRepoSub.VerifyParticipantAccess(Arg.Any<int>(), Arg.Any<int>()).Returns(true);

        sessionRepoSub.GetSessionOverviewAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult<Session?>(null));
        
        var result = await sessionService.GetSessionByIdAsync(1, 1, Roles.AnonymousUser);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task GetSessionByIdAsync_ShouldReturn_Ok()
    {
        var loggerSub = Substitute.For<ILogger<SessionService>>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();
        var tokenServiceSub = Substitute.For<IAnonTokenService>();
        var exerciseRepoSub = Substitute.For<IExerciseRepository>();
        var sessionService = new SessionService(sessionRepoSub, loggerSub, tokenServiceSub);
        sessionRepoSub.VerifyParticipantAccess(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var session = new Session();
        sessionRepoSub.GetSessionOverviewAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(session);
        
        var result = await sessionService.GetSessionByIdAsync(1, 1, Roles.AnonymousUser);
        
        Assert.True(result.IsSuccess);
    }
    
    
}