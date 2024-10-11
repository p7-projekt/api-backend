using FluentValidation;

namespace Core.Example;

public class ExampleDtoValidator : AbstractValidator<ExampleDto>
{
    public ExampleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Name).MinimumLength(1).MaximumLength(100).WithMessage("Name must be between 1 and 100 characters");
        RuleFor(x => x.Age).NotEmpty().WithMessage("Age is required");
        RuleFor(x => x.Age).GreaterThan(15).WithMessage("Age must be greater than 15");
        RuleFor(x => x.Age).LessThanOrEqualTo(100).WithMessage("Age must be less than or equal to 100");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email is incorrect");
        RuleFor(x => x.Surname).NotEmpty().WithMessage("Surname is required");
        RuleFor(x => x.Surname).MinimumLength(1).MaximumLength(100).WithMessage("Name must be between 1 and 100 characters");

    }

    public bool ExampleDtoValidationFilter(ExampleDto dto, out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        var firstnameErrors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            firstnameErrors.Add("Name is required");
        }
        if(firstnameErrors.Any()) errors.Add(nameof(dto.Name), firstnameErrors.ToArray());
        // ....
        
        if(errors.Any()) return false;
        
        return true;
    }
}