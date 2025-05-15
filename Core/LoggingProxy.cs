using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Momkay.LoggingProxy.Attributes;

namespace Momkay.LoggingProxy.Core;

internal class LoggingProxy<T> : DispatchProxy
{
    private T _decorated = default!;
    private ILogger<T> _logger = default!;

    public void SetParameters(T decorated, ILogger<T> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod.GetCustomAttribute<NoLogAttribute>() != null)
            return targetMethod.Invoke(_decorated, args);

        var level = targetMethod.GetCustomAttribute<LogAttribute>()?.Level ?? LogLevel.Information;
        var name = $"{typeof(T).Name}.{targetMethod.Name}";
        var argsJson = JsonSerializer.Serialize(args);
        var stopwatch = Stopwatch.StartNew();

        _logger.Log(level, "{Method} called with args: {Args}", name, argsJson);

        try
        {
            var result = targetMethod.Invoke(_decorated, args);
            stopwatch.Stop();
            _logger.Log(level, "{Method} completed in {Elapsed}ms", name, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (TargetInvocationException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex.InnerException ?? ex, "{Method} failed after {Elapsed}ms", name, stopwatch.ElapsedMilliseconds);
            throw ex.InnerException ?? ex;
        }
    }
}
