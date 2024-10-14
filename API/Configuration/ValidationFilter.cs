using FluentValidation;

namespace API.Configuration;

public class ValidationFilter<TModel> : IEndpointFilter 
{
    private readonly IValidator<TModel> _validationFilter;
    private readonly ILogger<ValidationFilter<TModel>> _logger;

    public ValidationFilter(ILogger<ValidationFilter<TModel>>  logger, IValidator<TModel> validationFilter)
    {
        _logger = logger;
        _validationFilter = validationFilter;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TModel>().First();
        var result = await _validationFilter.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            _logger.LogInformation("Validation error occured for {connectionId}", context.HttpContext.Connection.Id);
            return TypedResults.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithRequestValidation<TModel>(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ValidationFilter<TModel>>().ProducesValidationProblem();
    }
}