using Core.Dashboards.Models;
using FluentResults;

namespace Core.Dashboards.Contracts
{
    public interface IDashboardService
    {
        Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInTimedSession(int sessionId, int userId);
        Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInclassSession(int sessionId, int userId);
        Task<Result<GetExerciseSolution>> GetExerciseSolution(int exerciseId, int userId);

    }
}
