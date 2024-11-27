using Core.Dashboards.Models;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dashboards.Contracts
{
    public interface IDashboardRepository
    {
        Task<IEnumerable<GetExercisesInSessionResponseDto>?> GetExercisesInTimedSessionBySessionIdAsync(int sessionId);
        Task<IEnumerable<GetExercisesInSessionResponseDto>?> GetExercisesInClassSessionBySessionIdAsync(int sessionId);
        Task<int> GetConnectedTimedUsersAsync(int sessionId);
        Task<int> GetConnectedUsersClassAsync(int sessionId);
        Task<bool> CheckSessionInClassroomAsync(int sessionId);
        Task<Result<GetExerciseSolutionResponseDto>> GetSolutionByUserIdAsync(int exerciseId,  int userId);
        Task<bool> CheckAutherizedToGetSolution (int exerciseId, int appUserId,  int userId);
    }
}
