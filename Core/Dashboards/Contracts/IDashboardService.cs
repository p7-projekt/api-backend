using Core.Dashboards.Models;
using FluentResults;

namespace Core.Dashboards.Contracts
{
    public interface IDashboardService
    {
        Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInTimedSession(int sessionId, int userId);
        Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInClassSession(int sessionId, int userId);
        Task<Result<GetExerciseSolutionResponseDto>> GetExerciseSolution(int exerciseId, int userId);

    }
}
