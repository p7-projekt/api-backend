using Core.Exercises.Models;

namespace Core.Solutions.Contracts;

public interface ISolutionRepository
{
	Task<List<Testcase>?> GetTestCasesByExerciseIdAsync(int exerciseId);
	Task<bool> CheckAnonUserExistsInSessionAsync(int userId);
	Task<bool> InsertSolvedRelation(int userId, int exerciseId);
}