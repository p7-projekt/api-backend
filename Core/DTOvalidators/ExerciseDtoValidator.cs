using Core.DTOs;
using Core.Example;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOvalidators;
public class ExerciseDtoValidator : AbstractValidator<ExerciseDto>
{
    public ExerciseDtoValidator()
    {
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
        catch (NullReferenceException)
        {
            return false;
        }
        return true;
    }

    private bool HaveAllParameters(List<(string[], string[])> testcases)
    {
        foreach (var testcase in testcases)
        {
            if (testcase.Item1 == null || testcase.Item1.Length == 0)
            {
                Console.WriteLine("Empty paramter");
                return false;
            }
            if (testcase.Item2 == null || testcase.Item2.Length == 0)
            {
                Console.WriteLine("Empty paramter");
                return false;
            }
        }
        return true;
    }

    private bool HasSameParameterAmount(List<(string[], string[])> testcases)
    {
        try
        {
            var temp = testcases.First();
            var inputParams = temp.Item1.Length;
            var outputParams = temp.Item2.Length;
            foreach (var testcase in testcases)
            {
                if (testcase.Item1.Length != inputParams || testcase.Item2.Length != outputParams)
                {
                    Console.WriteLine("Inconsistency in parameter amount across test cases");
                    return false;
                }
            }
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Missing testcases");
            return false;
        }
        catch (NullReferenceException)
        {
            Console.WriteLine("Missing testcases");
            return false;
        }
        return true;
    }

    private bool ParametersValuesHaveCorrectTypes(ExerciseDto dto)
    {
        foreach(var testcase in dto.Testcases)
        {
            try
            {
                for (int i = 0; i < dto.InputParameterType.Length; i++)
                {
                    switch (dto.InputParameterType[i].ToLower())
                    {
                        case "bool": var tempInBool = bool.Parse(testcase.Item1[i]); break;
                        case "int": var tempInInt = int.Parse(testcase.Item1[i]); break;
                        case "float": var tempInFloat = float.Parse(testcase.Item1[i]); break;
                        case "string": break;
                        case "char": if(testcase.Item1[i].Length != 1) { return false; }; break;
                        default: Console.WriteLine("Invalid input"); return false;
                    }
                }
                for (int i = 0; i < dto.OutputParamaterType.Length; i++)
                {
                    switch (dto.OutputParamaterType[i].ToLower())
                    {
                        case "bool": var tempOutBool = bool.Parse(testcase.Item2[i]); break;
                        case "int": var tempOutInt = int.Parse(testcase.Item2[i]); break;
                        case "float": var tempOutFloat = float.Parse(testcase.Item2[i]); break;
                        case "string": break;
                        case "char": if (testcase.Item2[i].Length != 1) { return false; }; break;
                        default: Console.WriteLine("Invalid output"); return false;
                    }
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid parameter format");
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Missing parameter");
                return false;
            }
            catch
            {
                // Log error.
                throw;
            }
        }
        return true;
    }
}

