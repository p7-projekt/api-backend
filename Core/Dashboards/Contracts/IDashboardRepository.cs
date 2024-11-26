﻿using Core.Dashboards.Models;
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
        Task<IEnumerable<GetExercisesInSessionResponseDto>?> GetExercisesInTimedSessionAsync(int sessionId);
        Task<IEnumerable<GetExercisesInSessionResponseDto>?> GetExercisesInClassSessionAsync(int sessionId);
        Task<int> GetConnectedTimedUsersAsync(int sessionId);
        Task<int> GetConnectedUsersClassAsync(int sessionId);
        Task<Result<GetExerciseSolutionResponseDto>> GetSolutionByIdAsync(int exerciseId,  int userId);
    }
}
