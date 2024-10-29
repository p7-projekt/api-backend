using Core.Exercises.Models;

namespace Core.Solutions.Contracts;

public interface ISolutionRepository
{
	Task<List<TestCaseEntity>?> GetTestCasesByExerciseIdAsync(int exerciseId);
}