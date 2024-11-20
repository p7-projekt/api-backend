using Core.Languages.Models;
using Core.Solutions.Models;
using FluentResults;

namespace Core.Solutions.Contracts;

public interface IMozartService
{
	Task<Result<SolutionRunnerResponse>> SubmitSubmission(SubmissionDto submission, Language language);
}