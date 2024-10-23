using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Exercises.Models;

namespace Core.Contracts.Services;

public interface ISolutionRunnerService
{
    Task SubmitSolutionAsync(ExerciseDto dto);

}
