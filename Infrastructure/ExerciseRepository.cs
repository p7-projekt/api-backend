using Core.Contracts.Repositories;
using Core.Models;
using Core.Models.DTOs;
using Dapper;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<string> InsertExerciseAsync(ExerciseDto dto, int userId)
        {
            using var con = await _connection.CreateConnectionAsync();
            using var transaction = con.BeginTransaction();

            var exerciseQuery = """
                                INSERT INTO exercise (author_id, title, description, solution) VALUES (@Author, @Title, @Description, @Solution) RETURNING exercise_id;
                                """;
            var exerciseId = await con.ExecuteScalarAsync<int>(exerciseQuery,
                new {
                    Author = userId,
                    Title = dto.Name,
                    Description = dto.Description,
                    Solution = dto.Solution
                }, transaction);

            var insertTestcasesQuery = """
                                       INSERT INTO testcase (exercise_id, testcase_no) VALUES (@ExerciseId, @OrderNo) RETURNING testcase_id;
                                       """;

            List<int> OrderNo = new List<int>();
            for (int i = 0; i < dto.Testcases.Count(); i++)
            {
                OrderNo.Add(i);
            }

            await con.ExecuteAsync(insertTestcasesQuery,
                OrderNo.Select(i => new { ExerciseId = exerciseId, OrderNo = i}).ToList(),
                transaction);

            var getTestcasesIdsQuery = """
                                       SELECT testcase_id FROM testcase WHERE exercise_id=@ExerciseId;
                                       """;

            var testcasesIds = con.Query<int>(getTestcasesIdsQuery,
                new { ExerciseId = exerciseId },
                transaction);

            var insertParameterQuery = """
                                       INSERT INTO testcase_parameter (testcase_id, arg_num, parameter_type, parameter_value, is_output) 
                                                               VALUES (@TestcaseId, @OrderNo, @ParamType, @Value, @IsOutput)
                                       """;

            for(var i = 0; i < testcasesIds.Count(); i++)
            {
                var InputOrderAndTypeAndValue = new List<(int, string, string)>();
                for (int j = 0; j < dto.InputParameterType.Length; j++)
                {
                    InputOrderAndTypeAndValue.Add((j, dto.InputParameterType[j],dto.Testcases.ElementAt(i).inputParams[j]));
                }
                var OutputOrderAndTypeAndValue = new List<(int, string, string)>();
                for (int j = 0; j < dto.OutputParamaterType.Length; j++)
                {
                    OutputOrderAndTypeAndValue.Add((j, dto.OutputParamaterType[j], dto.Testcases.ElementAt(i).outputParams[j]));
                }
                await con.ExecuteAsync(insertParameterQuery,
                    InputOrderAndTypeAndValue.Select(x => new { TestcaseId = testcasesIds.ElementAt(i), 
                                                         OrderNo = x.Item1, 
                                                         ParamType = x.Item2, 
                                                         Value = x.Item3, 
                                                         IsOutput = false }),
                    transaction);
                await con.ExecuteAsync(insertParameterQuery,
                    OutputOrderAndTypeAndValue.Select(x => new { TestcaseId = testcasesIds.ElementAt(i), 
                                                          OrderNo = x.Item1, 
                                                          ParamType = x.Item2, 
                                                          Value = x.Item3, 
                                                          IsOutput = true }),
                    transaction);
            }

            transaction.Commit();

            //Add transaction rollbacks

            return "";
        }
    }
}
