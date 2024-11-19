using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Solutions.Models;

namespace Core.Solutions.Contracts;

public interface ISolutionRepository
{
	Task<List<Testcase>?> GetTestCasesByExerciseIdAsync(int exerciseId);
	Task<bool> CheckAnonUserExistsInSessionAsync(int userId, int sessionId);
	Task<bool> InsertSolvedRelation(int userId, int exerciseId, int sessionId);
	
	Task<LanguageSupport?> GetSolutionLanguageBySession(int languageId, int sessionId);

	Task<bool> InsertSubmissionRelation(Submission submission);

}