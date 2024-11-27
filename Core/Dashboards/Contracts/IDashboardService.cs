using Core.Dashboards.Models;
using FluentResults;

namespace Core.Dashboards.Contracts
{
    public interface IDashboardService
    {
        Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInSession(int sessionId, int userId);
        Task<Result<GetExerciseSolutionResponseDto>> GetUserSolution(int exerciseId, int appUserId, int userId);

    }
}
