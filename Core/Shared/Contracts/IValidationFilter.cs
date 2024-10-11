namespace Core.Shared.Contracts;

public interface IValidationFilter<TModel>
{
    bool Validate(TModel model, out Dictionary<string, string[]> errors);
}