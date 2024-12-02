using Core.Dashboards.Contracts;
using Core.Dashboards.Models;
using Core.Sessions.Contracts;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Core.Dashboards;

public class DashboardService : IDashboardService
{
    private readonly ILogger<DashboardService> _logger;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ISessionRepository _sessionRepository;
    public DashboardService(ILogger<DashboardService> logger, IDashboardRepository dashboardRepository, ISessionRepository sessionRepository)
    {
        _logger = logger;
        _dashboardRepository = dashboardRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInSession(int sessionId, int userId)
    {
        var access = false;
        access = await _sessionRepository.VerifyAuthor(userId, sessionId);
        if (!access)
        {
            _logger.LogInformation("User {userid} does not have access to {sessionid}", userId, sessionId);
            return Result.Fail("User does not have access to session");
        }
        var inClassroom = await _dashboardRepository.CheckSessionInClassroomAsync(sessionId);
        int usersConnected;

        if (!inClassroom)
        {
            usersConnected = await _dashboardRepository.GetConnectedTimedUsersAsync(sessionId);
        }
        else
        {
            usersConnected = await _dashboardRepository.GetConnectedUsersClassAsync(sessionId);
        }
        
        var exercises = await _dashboardRepository.GetExercisesInSessionBySessionIdAsync(sessionId);
        if (exercises == null || exercises.Count() == 0)
        {
            _logger.LogInformation("No Exercises in timed session: {sessionID}", sessionId);
            return Result.Fail("Exercises not found");
        }
        return Result.Ok(TransformExercisesInSessionDto(exercises, usersConnected));

    }

    public async Task<Result<GetExerciseSolutionResponseDto>> GetUserSolution(int exerciseId, int appUserId, int userId)
    {
        var authorized = await _dashboardRepository.CheckAuthorizedToGetSolution(exerciseId, appUserId, userId);

        if (!authorized)
        {
            _logger.LogInformation("User {userId} not autherized to access solution for exercise {exerciseId} by appuser {appUserId}", userId, exerciseId, appUserId);
            return Result.Fail("Not authorized");
        }

        var solution = await _dashboardRepository.GetSolutionByUserIdAsync(exerciseId, appUserId);
        if (solution.IsFailed)
        {
            _logger.LogInformation("Could not find solution with exercise id: {exerciseID} by user {UserID}", exerciseId, appUserId);
            return Result.Fail("Not found");
        }
        return solution;
    }

    private GetExercisesInSessionCombinedInfo TransformExercisesInSessionDto(IEnumerable<GetExercisesInSessionResponseDto> exercises, int usersConnected)
    {
        var combinedDtos = exercises.Select(dto => new GetExercisesAndUserDetailsInSessionResponseDto(
        dto.Title,
        dto.Id,
        dto.Solved,
        dto.Attempted,
        dto.UserIds.Zip(dto.Names, (id, name) => new UserDetailDto(id, name)).ToList())).ToList();

        var combinedInfoDto = new GetExercisesInSessionCombinedInfo(usersConnected, combinedDtos);
        return combinedInfoDto;
    }
}
