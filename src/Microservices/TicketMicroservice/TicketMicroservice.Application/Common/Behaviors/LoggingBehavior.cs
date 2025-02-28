using MediatR;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TicketMicroservice.Application.Common.Behaviors;

/// <summary>
/// Logs every request that goes through MediatR.
/// </summary>
[DebuggerStepThrough]
internal class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling request {RequestName} with data: {@Request}",
            typeof(TRequest).Name,
            request);

        var response = await next();

        logger.LogInformation(
            "Handled request {RequestName} with response: {@Response}",
            typeof(TResponse).Name,
            response);

        return response;
    }
}