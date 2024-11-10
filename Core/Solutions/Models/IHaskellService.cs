using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Solutions.Models;

public interface IHaskellService
{
    Task<Result<SolutionRunnerResponse>> SubmitSubmission(SubmissionDto submission);
}
