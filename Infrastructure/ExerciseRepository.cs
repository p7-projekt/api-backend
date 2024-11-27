using System.Data;
using Dapper;
using FluentResults;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using Core.Exercises.Models;
using Core.Exercises.Contracts;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Infrastructure
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly IDbConnectionFactory _connection;
        private readonly ILogger<ExerciseRepository> _logger;

        public ExerciseRepository(IDbConnectionFactory connection, ILogger<ExerciseRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<bool> VerifyExerciseAuthorAsync(int exerciseId, int authorId)
        {
            using var con = await _connection.CreateConnectionAsync();
            var query = """
                        SELECT COUNT(*) FROM EXERCISE WHERE exercise_id = @ExerciseId AND author_id = @AuthorId;
                        """;
            var result = await con.QueryFirstOrDefaultAsync<int>(query, new { ExerciseId = exerciseId, @AuthorID = authorId });
            return result == 1;
        }

        public async Task<Result> UpdateExerciseAsync(ExerciseDto dto, int exerciseId)
        {
            using var con = await _connection.CreateConnectionAsync();
            using var transaction = con.BeginTransaction();

            var exerciseQuery = """
                                UPDATE exercise 
                                SET title = @Title, description = @Description, solution = @Solution
                                WHERE exercise_id = @ExerciseId;
                                """;
            var result = await con.ExecuteAsync(exerciseQuery,
                new
                {
                    Title = dto.Name,
                    Description = dto.Description,
                    Solution = dto.Solution,
                    ExerciseId = exerciseId
                }, transaction);

            if(result != 1)
            {
                transaction.Rollback();
                return Result.Fail("Exercise was not updated");
            }

            var deleteTestcasesQuery = """
                                       DELETE from testcase WHERE exercise_id = @ExerciseId;
                                       """;
            await con.ExecuteAsync(deleteTestcasesQuery, new { ExerciseId = exerciseId }, transaction);

            var Inserts = await InsertTestcasesOfExercise(dto, exerciseId, con, transaction);

            if (Inserts.Item1 != dto.Testcases.Count)
            {
                _logger.LogError("Error inserting testcases - Expected {total_testcases} - Actual {inserted_testcases}", dto.Testcases.Count, Inserts.Item1);
                transaction.Rollback();
                return Result.Fail("Error inserting testcases");
            }
            var totalParams = 0;
            totalParams += dto.Testcases.Sum(x => x.InputParams.Count());
            totalParams += dto.Testcases.Sum(x => x.OutputParams.Count());
            if (Inserts.Item2 != totalParams)
            {
                _logger.LogError("Error inserting parameters - Expected {total_params} - Actual {inserted_params}", totalParams, Inserts.Item2);
                transaction.Rollback();
                return Result.Fail("Error inserting parameters ");
            }

            transaction.Commit();

            return Result.Ok();

        }

        public async Task<Result> InsertExerciseAsync(ExerciseDto dto, int authorId)
        {

            using var con = await _connection.CreateConnectionAsync();
            using var transaction = con.BeginTransaction();
            try
            {
                var exerciseQuery = """
                                INSERT INTO exercise (author_id, title, description, solution, solution_language_id) VALUES (@Author, @Title, @Description, @Solution, @LanguageId) RETURNING exercise_id;
                                """;
                var exerciseId = await con.ExecuteScalarAsync<int>(exerciseQuery,
                    new
                    {
                        Author = authorId,
                        Title = dto.Name,
                        Description = dto.Description,
                        Solution = dto.Solution,
                        LanguageId = dto.SolutionLanguage
                    }, transaction);

                var Inserts = await InsertTestcasesOfExercise(dto, exerciseId, con, transaction);

                if(Inserts.Item1 != dto.Testcases.Count)
                {
                    _logger.LogError("Error inserting testcases - Expected {total_testcases} - Actual {inserted_testcases}", dto.Testcases.Count, Inserts.Item1);
                    return Result.Fail("Error inserting testcases");
                }
                var totalParams = 0;
                totalParams += dto.Testcases.Sum(x => x.InputParams.Count());
                totalParams += dto.Testcases.Sum(x => x.OutputParams.Count());
                if (Inserts.Item2 != totalParams)
                {
                    _logger.LogError("Error inserting parameters - Expected {total_params} - Actual {inserted_params}", totalParams, Inserts.Item2);
                    return Result.Fail("Error inserting parameters ");
                }

                transaction.Commit();

            } catch (Exception ex) // Unsure what Exception may occur
            {
                _logger.LogError("Error occured during confirmation of new exercise or insertion into DB. Message: {}", ex.Message);
                transaction.Rollback();
                return Result.Fail(ex.Message);
            }

            return Result.Ok();
        }

        public async Task<IEnumerable<GetExercisesResponseDto>?> GetExercisesAsync(int authorId)
        {
            using var con = await _connection.CreateConnectionAsync();
            var query = """
                    SELECT exercise_id AS id, title as name FROM exercise WHERE author_id = @Id;
                    """;
            var results = await con.QueryAsync<GetExercisesResponseDto>(query, new { Id = authorId });
            return results;
        }

        public async Task<GetExerciseResponseDto?> GetExerciseByIdAsync(int exerciseId)
        {
            using var con = await _connection.CreateConnectionAsync();
            var query = """
                    SELECT exercise_id AS id, title, description, solution, solution_language_id AS languageid FROM exercise WHERE exercise_id = @ExerciseId;
                    """;
            var results = await con.QueryFirstOrDefaultAsync<GetExerciseResponseDto>(query, new { ExerciseId = exerciseId });

            return results;
        }

        public async Task<bool> DeleteExerciseAsync(int exerciseId)
        {
            using var con = await _connection.CreateConnectionAsync();
            var query = """
                    DELETE FROM exercise WHERE exercise_id = @ExerciseId;
                    """;
            var result = await con.ExecuteAsync(query, new { ExerciseId = exerciseId});
            return result == 1;
        }

        private async Task<(int, int)> InsertTestcasesOfExercise(ExerciseDto dto, int exerciseId, IDbConnection con, IDbTransaction transaction)
        {

            var insertTestcasesQuery = """
                                       INSERT INTO testcase (exercise_id, testcase_no, public_visible) VALUES (@ExerciseId, @OrderNo, @PublicVisible);
                                       """;

            var OrderNo = new List<(int, bool)>();
            for (int i = 0; i < dto.Testcases.Count(); i++)
            {
                OrderNo.Add((i, dto.Testcases[i].PublicVisible));
            }
            var insertedExercises = 0;
            // Inserts all testcases of an exercise
            insertedExercises += await con.ExecuteAsync(insertTestcasesQuery,
                OrderNo.Select(i => new { ExerciseId = exerciseId, OrderNo = i.Item1, PublicVisible = i.Item2 }).ToList(),
                transaction);

            var getTestcasesIdsQuery = """
                                       SELECT testcase_id FROM testcase WHERE exercise_id=@ExerciseId;
                                       """;

            // Retrieves ids of the previous inserted testcases
            var testcasesIds = con.Query<int>(getTestcasesIdsQuery,
                new { ExerciseId = exerciseId },
                transaction);

            var insertParameterQuery = """
                                       INSERT INTO testcase_parameter (testcase_id, arg_num, parameter_type, parameter_value, is_output) 
                                                               VALUES (@TestcaseId, @OrderNo, @ParamType, @Value, @IsOutput)
                                       """;
            var insertedParameters = 0;
            // Inserts parameters for one testcase per iteration.
            for (var i = 0; i < testcasesIds.Count(); i++)
            {
                // Combines the order of the paramater, with the respective type and value in a shared list.
                // Reason being that they are iterated over together in the subsequent query.
                var InputParamDataStructure = ConstructTestcaseParameterQueryArgument(dto.Testcases.ElementAt(i), dto.InputParameterType, IsOutput: false);
                var OutputParamDataStructure = ConstructTestcaseParameterQueryArgument(dto.Testcases.ElementAt(i), dto.OutputParamaterType, IsOutput: true);
                insertedParameters += await con.ExecuteAsync(insertParameterQuery,
                    InputParamDataStructure.Select(x => new
                    {
                        TestcaseId = testcasesIds.ElementAt(i),
                        OrderNo = x.Item1,
                        ParamType = x.Item2.ToLower(),
                        Value = x.Item3,
                        IsOutput = false
                    }),
                                transaction);
                insertedParameters += await con.ExecuteAsync(insertParameterQuery,
                    OutputParamDataStructure.Select(x => new
                    {
                        TestcaseId = testcasesIds.ElementAt(i),
                        OrderNo = x.Item1,
                        ParamType = x.Item2.ToLower(),
                        Value = x.Item3,
                        IsOutput = true
                    }),
                    transaction);
            }
            return (insertedExercises, insertedParameters);
        }

        private List<(int, string, string)> ConstructTestcaseParameterQueryArgument(TestcaseDto tc, string[] paramType, bool IsOutput)
        {
            var OrderAndTypeAndValue = new List<(int, string, string)>();
            string[] paramsDecider;
            if (IsOutput)
            {
                paramsDecider = tc.OutputParams;
            } else
            {
                paramsDecider = tc.InputParams;
            }
            for (int j = 0; j < paramType.Length; j++)
            {
                OrderAndTypeAndValue.Add((j, paramType[j], paramsDecider[j]));
            }
            return OrderAndTypeAndValue;
        }
    }
}
