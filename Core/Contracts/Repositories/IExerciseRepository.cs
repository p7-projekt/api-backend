using Core.Models.DTOs;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Repositories;

public interface IExerciseRepository
{
    Task<Result> InsertExerciseAsync(ExerciseDto dto, int userId);
}
