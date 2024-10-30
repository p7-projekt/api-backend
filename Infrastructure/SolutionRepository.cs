using Core.Exercises.Models;
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

	public async Task<List<TestCaseEntity>?> GetTestCasesByExerciseIdAsync(int exerciseId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		try
		{
			var query = """
			            SELECT testcase_id AS testcaseid, exercise_id as exerciseid, testcase_no as testcasenumber FROM testcase WHERE exercise_id = @ExerciseId;
			            """;
			var testCases = (await con.QueryAsync<TestCaseEntity>(query, new { exerciseId })).ToList();
			
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
				var inputList = (await con.QueryAsync<TestParameterEntity>(
					inputParameterQuery, 
					new {testcase.TestCaseId})).ToList();
        
				var outputList = (await con.QueryAsync<TestParameterEntity>(
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

	public async Task<bool> CheckAnonUserExistsInSessionAsync(int userId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
		var query = """
		            SELECT COUNT(*) FROM exercise_in_session AS eis
		                JOIN anon_users AS au
		                ON au.session_id = eis.session_id
		            WHERE au.user_id = @UserId;
		            """;
		var result = await con.ExecuteScalarAsync<int>(query, new { UserId = userId });
		return result > 0;
	}

	public async Task<bool> InsertSolvedRelation(int userId, int exerciseId)
	{
		using var con = await _dbConnection.CreateConnectionAsync();
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