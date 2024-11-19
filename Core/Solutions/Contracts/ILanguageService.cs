using Core.Solutions.Models;
using FluentResults;

namespace Core.Solutions.Contracts;

public interface ILanguageService
{
	Task<Result<SolutionRunnerResponse>> SubmitSubmission(SubmissionDto submission, Language language);
}