using Core.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Services;

public interface ISolutionRunnerService
{
    Task SubmitSolutionAsync(ExerciseDto dto);

}
