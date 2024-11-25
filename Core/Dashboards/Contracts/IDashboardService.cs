using Core.Dashboards.Models;
using FluentResults;

namespace Core.Dashboards.Contracts
{
    public interface IDashboardService
    {
        Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInTimedSessionAsync(int sessionId, int userId);
        Task<Result<GetExercisesInSessionCombinedInfo>> GetExercisesInclassSessionAsync(int sessionId, int userId);

    }
}
