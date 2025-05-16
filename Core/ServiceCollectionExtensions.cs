using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;

namespace Momkay.LoggingProxy.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoggedServices(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Impl = t,
                Interface = t.GetInterfaces().SingleOrDefault(i => i.Name == $"I{t.Name}")
            })
            .Where(x => x.Interface != null);

        foreach (var t in types)
        {
            services.AddScoped(t.Impl);
            services.AddScoped(t.Interface!, sp =>
            {
                var impl = sp.GetRequiredService(t.Impl);
                var loggerType = typeof(ILogger<>).MakeGenericType(t.Interface!);
                var logger = (ILogger)sp.GetRequiredService(loggerType);
                var method = typeof(LoggingProxyFactory).GetMethod(nameof(LoggingProxyFactory.Create))!.MakeGenericMethod(t.Interface!);
                return method.Invoke(null, [impl, logger])!;
            });
        }

        return services;
    }
}