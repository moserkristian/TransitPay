using MediatR;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TicketMicroservice.Application.Common.Behaviors;

namespace TicketMicroservice.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBehaviors(
        this IServiceCollection services)
    {
        services.AddMediatR(
            cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }
}