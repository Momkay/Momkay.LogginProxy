using System;
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

    private const string ColorCyan = "\u001b[36m";
    private const string ColorGreen = "\u001b[32m";
    private const string ColorRed = "\u001b[31m";
    private const string ColorReset = "\u001b[0m";

    public void SetParameters(T decorated, ILogger<T> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    protected override object? Invoke(MethodInfo targetMethod, object?[]? args)
    {
        if (targetMethod.GetCustomAttribute<NoLogAttribute>() != null)
            return targetMethod.Invoke(_decorated, args);

        var level = targetMethod.GetCustomAttribute<LogAttribute>()?.Level ?? LogLevel.Information;
        var name = $"{typeof(T).Name}.{targetMethod.Name}";
        var argsJson = JsonSerializer.Serialize(args);
        var stopwatch = Stopwatch.StartNew();

        _logger.Log(level, $"{ColorCyan}[LOGGING-PROXY] {name} called with args: {argsJson}{ColorReset}");

        try
        {
            var result = targetMethod.Invoke(_decorated, args);

            if (result is Task taskResult)
            {
                if (targetMethod.ReturnType.IsGenericType &&
                    targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var taskType = targetMethod.ReturnType.GetGenericArguments()[0];
                    var method = typeof(LoggingProxy<T>)
                        .GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
                        .MakeGenericMethod(taskType);

                    return method.Invoke(this, new object[] { result, level, name, stopwatch })!;
                }

                return InterceptAsync(taskResult, level, name, stopwatch);
            }

            stopwatch.Stop();
            _logger.Log(level, $"{ColorGreen}[LOGGING-PROXY] {name} completed in {stopwatch.ElapsedMilliseconds}ms{ColorReset}");
            return result;
        }
        catch (TargetInvocationException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex.InnerException ?? ex, $"{ColorRed}[LOGGING-PROXY] {name} failed after {stopwatch.ElapsedMilliseconds}ms{ColorReset}");
            throw ex.InnerException ?? ex;
        }
    }

    private async Task InterceptAsync(Task task, LogLevel level, string name, Stopwatch stopwatch)
    {
        try
        {
            await task;
            stopwatch.Stop();
            _logger.Log(level, $"{ColorGreen}[LOGGING-PROXY] {name} completed in {stopwatch.ElapsedMilliseconds}ms{ColorReset}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, $"{ColorRed}[LOGGING-PROXY] {name} failed after {stopwatch.ElapsedMilliseconds}ms{ColorReset}");
            throw;
        }
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(Task<TResult> task, LogLevel level, string name, Stopwatch stopwatch)
    {
        try
        {
            var result = await task;
            stopwatch.Stop();
            _logger.Log(level, $"{ColorGreen}[LOGGING-PROXY] {name} completed in {stopwatch.ElapsedMilliseconds}ms{ColorReset}");
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, $"{ColorRed}[LOGGING-PROXY] {name} failed after {stopwatch.ElapsedMilliseconds}ms{ColorReset}");
            throw;
        }
    }
}