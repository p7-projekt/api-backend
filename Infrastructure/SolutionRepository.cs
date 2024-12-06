using Core.Exercises.Models;
using Core.Languages.Models;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Dapper;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class SolutionRepository : ISolutionRepository
{
	private readonly IDbConnectionFactory _dbConnection;
	private readonly ILogger<SolutionRepository> _logger;

	public SolutionRepository(IDbConnectionFactory dbConnection, ILogger<SolutionRepository> logger)
	{
		_dbConnection = dbConnection;
		_logger = logger;
	}

	public async Task<List<Testcase>?> GetTestCasesByExerciseIdAsync(int exerciseId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		try
		{
			var query = """
			            SELECT testcase_id AS testcaseid, exercise_id as exerciseid, testcase_no as testcasenumber,
			            public_visible as IsPublicVisible FROM testcase WHERE exercise_id = @ExerciseId;
			            """;
			var testCases = (await con.QueryAsync<Testcase>(query, new { exerciseId })).ToList();
			
			var inputParameterQuery = """
			                          SELECT parameter_id AS parameterid, testcase_id as testcaseid, arg_num as 
			                          argumentnumber, parameter_type as parametertype, parameter_value as parametervalue, is_output as isoutput
			                          FROM testcase_parameter WHERE testcase_id = @TestCaseId AND is_output = false
			                          ORDER BY arg_num;
			                          """;
			
			var outputParameterQuery = """
			                           SELECT parameter_id AS parameterid, testcase_id as testcaseid, arg_num as 
			                           argumentnumber, parameter_type as parametertype, parameter_value as parametervalue, is_output as isoutput
			                           FROM testcase_parameter WHERE testcase_id = @TestCaseId AND is_output = true
			                           ORDER BY arg_num;
			                           """;
			
			foreach (var testcase in testCases)
			{
				var inputList = (await con.QueryAsync<TestParameter>(
					inputParameterQuery, 
					new {testcase.TestCaseId})).ToList();
        
				var outputList = (await con.QueryAsync<TestParameter>(
					outputParameterQuery, 
					new { testcase.TestCaseId })).ToList();
				
				if (inputList.Count == 0 || outputList.Count == 0)
				{
					_logger.LogError("Test case {testcaseid} does not have any input/output parameters", testcase.TestCaseId);
					return null;
				}
				testcase.Input = inputList;
				testcase.Output = outputList;
			}
			return testCases;
		}
		catch (Exception e)
		{
			_logger.LogError("Could not retrieve testcases from exercise {exerciseid}, error msg: {exceptionmsg}", exerciseId, e.Message);
			return null;
		}
	}

	public async Task<bool> CheckUserAssociationToSessionAsync(int userId, int sessionId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		
		var timedSessionQuery = """
		            SELECT Count(*) FROM user_in_timedsession WHERE user_id = @UserID AND session_id = @SessionId
		            """;
		var timedSessionResult = await con.ExecuteScalarAsync<int>(timedSessionQuery, new { UserId = userId, SessionId = sessionId });

		var classroomSessionQuery = """
									SELECT Count(*)
									FROM student_in_classroom as stud
									JOIN session_in_classroom AS ses
										ON stud.classroom_id = ses.classroom_id
									WHERE student_id = @UserID AND session_id = @SessionId
									""";
		var classroomSessionResult = await con.ExecuteScalarAsync<int>(classroomSessionQuery, new { UserId = userId, SessionId = sessionId });

		return (timedSessionResult > 0) || (classroomSessionResult > 0);
	}

	public async Task<LanguageSupport?> GetSolutionLanguageBySession(int languageId, int sessionId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		// lis (session_id, language_id) ls (langauge_id, language, version) 
		var query = """
		            SELECT ls.language_id AS id, ls.language, ls.version 
		            FROM language_support AS ls
		            JOIN language_in_session AS lis
		            	on lis.language_id = ls.language_id
		            WHERE lis.session_id = @SessionId AND lis.language_id = @LanguageId
		            """;
		return await con.QuerySingleOrDefaultAsync<LanguageSupport>(query, new { LanguageId = languageId, SessionId = sessionId });
	}

	public async Task<bool> VerifyUserInTimedSession(int userId, int sessionId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		// verify correct session id in Submission and exercise is in that session
        	var verifySessionExerciseQuery = """
                                         SELECT COUNT(*)
                                         FROM user_in_timedsession
                                         WHERE session_id = @SessionId AND user_id = @UserId;
                                         """;
        var verification = await con.ExecuteScalarAsync<int>(verifySessionExerciseQuery,
        	new
        	{
        		 SessionId = sessionId, UserId = userId
        	});
        if (verification != 1)
        {
        	return false;
        }

        return true;
	}


	public async Task<bool> InsertSubmissionRelation(Submission submission) 
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		using var transaction = con.BeginTransaction();
		try
		{
			// check if there already exist a solution / submission
			if (submission.Solved)
			{
				var existingSubmissionQuery = """
				                              DELETE FROM submission
				                              WHERE exercise_id = @ExerciseId 
				                              AND user_id = @UserId 
				                              AND session_id = @SessionId;	
				                              """;
				
				await con.ExecuteAsync(existingSubmissionQuery, new {ExerciseId = submission.ExerciseId, UserId = submission.UserId, SessionId = submission.SessionId}, transaction);
			}
			else
			{
				var existingSubmissionQuery = """
			                                 SELECT COUNT(*) FROM submission
			                                 WHERE exercise_id = @ExerciseId
			                                 AND user_id = @UserId
			                                 AND session_id = @SessionId;
			                              """;
				var submissions = await con.QuerySingleOrDefaultAsync<int>(existingSubmissionQuery, new {ExerciseId = submission.ExerciseId, UserId = submission.UserId, SessionId = submission.SessionId}, transaction);
				if (submissions > 0)
				{
					transaction.Rollback();
					return true;
				}
			}

			// Insert 
			var insertSubmissionQuery = """
			                            INSERT INTO submission (user_id, session_id, exercise_id, solution, language_id, solved)
			                            VALUES
			                            (@UserId, @SessionId, @ExerciseId, @Solution, @LanguageId, @Solved);
			                            """;
			var result = await con.ExecuteAsync(
				insertSubmissionQuery, new {submission.UserId, submission.SessionId,
					submission.ExerciseId, submission.Solution, submission.LanguageId, submission.Solved}, transaction);
			if (result != 1)
			{
				transaction.Rollback();
				return false;
			}
			
			transaction.Commit();
		}
		catch (Exception)
		{
			_logger.LogError("Error inserting submission for user {user} for session {sessionid} at exercise {exerciseid}",
				submission.UserId, submission.SessionId, submission.ExerciseId);
			transaction.Rollback();
			return false;
		}
		
		return true;
	}
	
	public async Task<bool> VerifyExerciseInSessionAsync(int sessionId, int exerciseId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();

		var query = "SELECT COUNT(*) FROM exercise_in_session WHERE session_id = @SessionId AND exercise_id = @ExerciseId";

		var result = await con.QuerySingleAsync<int>(query, new { SessionId = sessionId, ExerciseId = exerciseId });

		return result > 0;
	}
	
	public async Task<bool> InsertSolvedRelation(int userId, int exerciseId, int sessionId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		using var transaction = con.BeginTransaction();
		
		// check if already attempted / solved
		// if yes delete entry, then insert
		// if not create solution and submission
		var query = """
		            WITH In_Session AS (SELECT au.user_id AS userid, 
		                                       eis.session_id AS sessionid, eis.exercise_id AS exerciseid
		                                FROM exercise_in_session AS eis
		                                         JOIN anon_users AS au
		                                              ON au.session_id = eis.session_id
		                                WHERE au.user_id = @UserId AND eis.exercise_id = @ExerciseId
		                                AND NOT EXISTS(
		                                SELECT 1 FROM solved WHERE user_id = au.user_id AND solved.exercise_id = eis.exercise_id)
		                                
		            )
		            INSERT INTO solved (user_id, session_id, exercise_id)
		            SELECT userid, sessionid, exerciseid
		            FROM In_Session
		            RETURNING user_id;
		            """;
		var result = await con.ExecuteScalarAsync<int>(query, new { UserId = userId, ExerciseId = exerciseId });
		return result > 0;
	}
}