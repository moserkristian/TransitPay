using MediatR;
using FluentValidation;
using System.Diagnostics;

namespace TicketMicroservice.Application.Common.Behaviors;

/// <summary>
/// Ensures request data is valid before execution.
/// </summary>
[DebuggerStepThrough]
internal class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToArray();

        if (failures.Length > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
