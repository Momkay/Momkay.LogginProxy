using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Momkay.LoggingProxy.Core;

public static class LoggingProxyFactory
{
    public static T Create<T>(T instance, ILogger<T> logger) where T : class
    {
        var proxy = DispatchProxy.Create<T, LoggingProxy<T>>() as LoggingProxy<T>;
        proxy!.SetParameters(instance, logger);
        return proxy as T;
    }
}