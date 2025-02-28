﻿using MediatR;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TicketMicroservice.Application.Common.Behaviors;

/// <summary>
/// Tracks execution time for requests.
/// </summary>
[DebuggerStepThrough]
internal class PerformanceBehavior<TRequest, TResponse>(
    ILoggerFactory logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new();
    private readonly ILogger<TRequest> _logger = logger.CreateLogger<TRequest>();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds <= 500)
        {
            return response;
        }

        var requestName = typeof(TRequest).Name;

        _logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
            requestName, elapsedMilliseconds, request);

        return response;
    }
}