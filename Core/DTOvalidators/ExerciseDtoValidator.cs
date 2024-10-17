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
        RuleFor(x => x.InputParameterType).NotEmpty().WithMessage("Type of input parameter must be provided");
        RuleFor(x => x.InputParameterType).Must(CheckType).WithMessage("Input parameter must be of a valid type");
        RuleFor(x => x.OutputParamaterType).NotEmpty().WithMessage("Type of output parameter must be provided");
        RuleFor(x => x.OutputParamaterType).Must(CheckType).WithMessage("Output parameter must be of a valid type");
        RuleFor(x => x.Testcases).Must(CheckTupleContent).WithMessage("All testcases must have both input and output");
        RuleFor(x => x.Testcases).Must(CheckParameterAmount).WithMessage("All testcases must have the same amount of parameters");
        RuleFor(x => x).Must()
    }

    private bool CheckType(string[] parameters)
    {
        foreach (var type in parameters)
        {
            switch (type.ToLower())
            {
                case "bool": break;
                case "int": break;
                case "float": break;
                case "double": break;
                case "string": break;
                case "char": break;
                default: return false;
            }
        }
        return true;
    }

    private bool CheckTupleContent(List<(string[], string[])> testcases)
    {
        foreach (var testcase in testcases)
        {
            if (testcase.Item1 == null || testcase.Item1.Length == 0)
            {
                return false;
            }
            if (testcase.Item2 == null || testcase.Item2.Length == 0)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckParameterAmount(List<(string[], string[])> testcases)
    {
        var temp = testcases.First();
        var inputParams = temp.Item1.Length;
        var outputParams = temp.Item2.Length;
        foreach (var testcase in testcases)
        {
            if (testcase.Item1.Length != inputParams || testcase.Item2.Length != outputParams)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckTestParameterType(ExerciseDto dto)
    {
        
    }
}

