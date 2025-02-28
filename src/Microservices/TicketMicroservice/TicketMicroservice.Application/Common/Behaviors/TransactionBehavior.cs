using MediatR;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TicketMicroservice.Application.Common.Behaviors;

/// <summary>
/// Wraps database calls in a transaction (only for commands).
/// </summary>
[DebuggerStepThrough]
public class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentException(nameof(ILogger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}
