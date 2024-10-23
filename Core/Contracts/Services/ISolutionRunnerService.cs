using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Exercises.Models;
using FluentResults;

namespace Core.Contracts.Services;

public interface ISolutionRunnerService
{
    Task<Result> SubmitSolutionAsync(ExerciseSubmissionDto dto);

}
