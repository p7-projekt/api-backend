using Core.Exercises.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Core.Exercises.Validators;
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
        RuleFor(x => x.InputParameterType).NotEmpty().WithMessage("Type of input parameter must be provided");
        RuleFor(x => x.InputParameterType).Must(ParametersAreValidType).WithMessage("Input parameter must be of a valid type");
        RuleFor(x => x.OutputParamaterType).NotEmpty().WithMessage("Type of output parameter must be provided");
        RuleFor(x => x.OutputParamaterType).Must(ParametersAreValidType).WithMessage("Output parameter must be of a valid type");
        RuleFor(x => x.Testcases).NotEmpty().WithMessage("Test cases must be provided");
        RuleFor(x => x.Testcases).Must(HaveAllParameters).WithMessage("All testcases must have both input and output");
        RuleFor(x => x.Testcases).Must(HasSameParameterAmount).WithMessage("All testcases must have the same amount of parameters");
        RuleFor(x => x).Must(ParametersValuesHaveCorrectTypes).WithMessage("All testcase parameters must be of correct type");
        RuleForEach(x => x.Testcases).ChildRules(testcase =>
        {
            testcase.RuleFor(y => y.PublicVisible).NotNull().WithMessage("The visibility of each testcase must be determined");
        });
        RuleFor(x => x.Testcases).Must(y => y.Any(z => z.PublicVisible)).WithMessage("At least one test case must be marked as publically visible");
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
                    default: _logger.LogInformation("Invalid parameter type"); return false;
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

    private bool HaveAllParameters(List<TestcaseDto> testcases)
    {
        foreach (var testcase in testcases)
        {
            if (testcase.InputParams == null || testcase.InputParams.Length == 0)
            {
                _logger.LogInformation("Empty input paramter for; {}", testcase);
                return false;
            }
            if (testcase.OutputParams == null || testcase.OutputParams.Length == 0)
            {
                _logger.LogInformation("Empty output paramter for; {}", testcase);
                return false;
            }
        }
        return true;
    }

    private bool HasSameParameterAmount(List<TestcaseDto> testcases)
    {
        try
        {
            var temp = testcases.First();
            var inputParams = temp.InputParams.Length;
            var outputParams = temp.OutputParams.Length;
            foreach (var testcase in testcases)
            {
                if (testcase.InputParams.Length != inputParams || testcase.OutputParams.Length != outputParams)
                {
                    _logger.LogInformation("Inconsistency in parameter amount across test cases");
                    return false;
                }
            }
        }
        catch (Exception ex)
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
                        case "bool": var tempInBool = bool.Parse(testcase.InputParams[i]); break;
                        case "int": var tempInInt = Int64.Parse(testcase.InputParams[i]); break;
                        case "float": var tempInFloat = double.Parse(testcase.InputParams[i]); break;
                        case "string": break;
                        case "char": if (testcase.InputParams[i].Length != 1) { _logger.LogInformation("Empty input param for testcase");  return false; }; break;
                        default: _logger.LogInformation("Invalid input"); return false;
                    }
                }
                for (int i = 0; i < dto.OutputParamaterType.Length; i++)
                {
                    switch (dto.OutputParamaterType[i].ToLower())
                    {
                        case "bool": var tempOutBool = bool.Parse(testcase.OutputParams[i]); break;
                        case "int": var tempOutInt = Int64.Parse(testcase.OutputParams[i]); break;
                        case "float": var tempOutFloat = double.Parse(testcase.OutputParams[i]); break;
                        case "string": break;
                        case "char": if (testcase.OutputParams[i].Length != 1) { _logger.LogInformation("Empty output param for testcase"); return false; }; break;
                        default: _logger.LogInformation("Invalid output"); return false;
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


