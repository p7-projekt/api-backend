using Core.Example;
using Core.Models.DTOs;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.DTOvalidators;
public class ExerciseDtoValidator : AbstractValidator<ExerciseDto>
{
    readonly public ILogger<ExerciseDtoValidator> _logger;
    public ExerciseDtoValidator(ILogger<ExerciseDtoValidator> logger)
    {
        _logger = logger;


        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Name).MinimumLength(1).MaximumLength(100).WithMessage("Exercise must have a title, that is no longer than 100 characters");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description of exercise is required");
        RuleFor(x => x.Description).MaximumLength(10000).WithMessage("The description is too long");
        RuleFor(x => x.Solution).NotEmpty().WithMessage("A proposed solution is required");
        RuleFor(x => x.Solution).MaximumLength(10000).WithMessage("The proposed solution is too long");
        RuleFor(x => x.InputParameterType).NotEmpty().NotNull().WithMessage("Type of input parameter must be provided");
        RuleFor(x => x.InputParameterType).Must(ParametersAreValidType).WithMessage("Input parameter must be of a valid type");
        RuleFor(x => x.OutputParamaterType).NotEmpty().NotNull().WithMessage("Type of output parameter must be provided");
        RuleFor(x => x.OutputParamaterType).Must(ParametersAreValidType).WithMessage("Output parameter must be of a valid type");
        RuleFor(x => x.Testcases).NotEmpty().NotNull().WithMessage("Test cases must be provided");
        RuleFor(x => x.Testcases).Must(HaveAllParameters).WithMessage("All testcases must have both input and output");
        RuleFor(x => x.Testcases).Must(HasSameParameterAmount).WithMessage("All testcases must have the same amount of parameters");
        RuleFor(x => x).Must(ParametersValuesHaveCorrectTypes).WithMessage("All testcase parameters must be of correct type");
    }

    private bool ParametersAreValidType(string[] parameters)
    {
        try
        {
            foreach (var type in parameters)
            {
                switch (type.ToLower())
                {
                    case "bool": break;
                    case "int": break;
                    case "float": break;
                    case "string": break;
                    case "char": break;
                    default: Console.WriteLine("Invalid parameter type"); return false;
                }
            }
        }
        catch (NullReferenceException ex)
        {
            _logger.LogInformation("Null encountered during parameter type validation. Message:{} ", ex.Message);
            return false;
        }
        return true;
    }

    private bool HaveAllParameters(List<Testcase> testcases)
    {
        foreach (var testcase in testcases)
        {
            if (testcase.inputParams == null || testcase.inputParams.Length == 0)
            {
                Console.WriteLine("Empty paramter");
                _logger.LogInformation("Empty input paramter for; {}", testcase);
                return false;
            }
            if (testcase.outputParams == null || testcase.outputParams.Length == 0)
            {
                Console.WriteLine("Empty paramter");
                _logger.LogInformation("Empty output paramter for; {}", testcase);
                return false;
            }
        }
        return true;
    }

    private bool HasSameParameterAmount(List<Testcase> testcases)
    {
        try
        {
            var temp = testcases.First();
            var inputParams = temp.inputParams.Length;
            var outputParams = temp.outputParams.Length;
            foreach (var testcase in testcases)
            {
                if (testcase.inputParams.Length != inputParams || testcase.outputParams.Length != outputParams)
                {
                    Console.WriteLine("Inconsistency in parameter amount across test cases");
                    return false;
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation("Missing testcases: {}", ex.Message);
            return false;
        }
        catch (NullReferenceException ex)
        {
            _logger.LogInformation("Missing testcases: {}", ex.Message);
            return false;
        }
        return true;
    }

    private bool ParametersValuesHaveCorrectTypes(ExerciseDto dto)
    {
        foreach (var testcase in dto.Testcases)
        {
            try
            {
                for (int i = 0; i < dto.InputParameterType.Length; i++)
                {
                    switch (dto.InputParameterType[i].ToLower())
                    {
                        case "bool": var tempInBool = bool.Parse(testcase.inputParams[i]); break;
                        case "int": var tempInInt = int.Parse(testcase.inputParams[i]); break;
                        case "float": var tempInFloat = float.Parse(testcase.inputParams[i]); break;
                        case "string": break;
                        case "char": if (testcase.inputParams[i].Length != 1) { _logger.LogInformation("Empty input param for testcase");  return false; }; break;
                        default: Console.WriteLine("Invalid input"); return false;
                    }
                }
                for (int i = 0; i < dto.OutputParamaterType.Length; i++)
                {
                    switch (dto.OutputParamaterType[i].ToLower())
                    {
                        case "bool": var tempOutBool = bool.Parse(testcase.outputParams[i]); break;
                        case "int": var tempOutInt = int.Parse(testcase.outputParams[i]); break;
                        case "float": var tempOutFloat = float.Parse(testcase.outputParams[i]); break;
                        case "string": break;
                        case "char": if (testcase.outputParams[i].Length != 1) { _logger.LogInformation("Empty output param for testcase"); return false; }; break;
                        default: Console.WriteLine("Invalid output"); return false;
                    }
                }
            }
            catch (FormatException ex)
            {
                _logger.LogInformation("Incorrect format of if testcase parameter. {}", ex.Message);
                return false;
            }
            catch (NullReferenceException ex)
            {
                _logger.LogInformation("Null encountered during ExcerciseDto validation: {}", ex.Message);
                return false;
            }
            catch (IndexOutOfRangeException ex)
            {
                _logger.LogInformation("Missing paramater. {}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unhandled exception caught in ExerciseDtoValidator: {}", ex.Message);
                throw;
            }
        }

        _logger.LogInformation("Exercise validated. Title: {}", dto.Name);
        return true;
    }
}

