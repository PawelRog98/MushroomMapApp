using FluentValidation;
using MediatR;

namespace MushroomMapApp.Features.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        if (_validators.Any())
        {
            var failures = _validators
                .Select(v=>v.Validate(context))
                .SelectMany(v=>v.Errors)
                .Where(x=>x != null)
                .ToList();
            
            if(failures.Any())
                throw new ValidationException(failures);
        }
        
        return await next();
    }
}