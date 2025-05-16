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

        if (LoggingProxyConfig.GlobalFilter?.Invoke(targetMethod) == false)
            return targetMethod.Invoke(_decorated, args);

        var attr = targetMethod.GetCustomAttribute<LogAttribute>();
        var level = attr?.Level ?? LoggingProxyConfig.DefaultLogLevel;
        var logReturn = attr?.IncludeReturnValue ?? LoggingProxyConfig.LogReturnValuesByDefault;

        var name = $"{typeof(T).Name}.{targetMethod.Name}";
        var argsJson = SafeSerialize(args);
        var stopwatch = Stopwatch.StartNew();

        var prefix = LoggingProxyConfig.EnableColors ? ColorCyan : "";
        var suffix = LoggingProxyConfig.EnableColors ? ColorReset : "";
        _logger.Log(level, $"{prefix}[LOGGING-PROXY ▶] {name} called with args: {argsJson}{suffix}");

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

                    return method.Invoke(this, new object[] { result, level, name, stopwatch, logReturn })!;
                }

                return InterceptAsync(taskResult, level, name, stopwatch);
            }

            stopwatch.Stop();
            var msg = $"{prefix}[LOGGING-PROXY ✔] {name} completed in {stopwatch.ElapsedMilliseconds}ms";
            if (logReturn && result != null)
                msg += $" with result: {SafeSerialize(result)}";
            msg += suffix;

            _logger.Log(level, msg);
            return result;
        }
        catch (TargetInvocationException ex)
        {
            stopwatch.Stop();
            var prefixErr = LoggingProxyConfig.EnableColors ? ColorRed : "";
            var suffixErr = LoggingProxyConfig.EnableColors ? ColorReset : "";
            _logger.LogError(ex.InnerException ?? ex, $"{prefixErr}[LOGGING-PROXY ❌] {name} failed after {stopwatch.ElapsedMilliseconds}ms{suffixErr}");
            throw ex.InnerException ?? ex;
        }
    }

    private async Task InterceptAsync(Task task, LogLevel level, string name, Stopwatch stopwatch)
    {
        try
        {
            await task;
            stopwatch.Stop();
            var prefix = LoggingProxyConfig.EnableColors ? ColorGreen : "";
            var suffix = LoggingProxyConfig.EnableColors ? ColorReset : "";
            _logger.Log(level, $"{prefix}[LOGGING-PROXY ✔] {name} completed in {stopwatch.ElapsedMilliseconds}ms{suffix}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var prefix = LoggingProxyConfig.EnableColors ? ColorRed : "";
            var suffix = LoggingProxyConfig.EnableColors ? ColorReset : "";
            _logger.LogError(ex, $"{prefix}[LOGGING-PROXY ❌] {name} failed after {stopwatch.ElapsedMilliseconds}ms{suffix}");
            throw;
        }
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(Task<TResult> task, LogLevel level, string name, Stopwatch stopwatch, bool logReturn)
    {
        try
        {
            var result = await task;
            stopwatch.Stop();
            var prefix = LoggingProxyConfig.EnableColors ? ColorGreen : "";
            var suffix = LoggingProxyConfig.EnableColors ? ColorReset : "";
            var msg = $"{prefix}[LOGGING-PROXY ✔] {name} completed in {stopwatch.ElapsedMilliseconds}ms";
            if (logReturn)
                msg += $" with result: {SafeSerialize(result)}";
            msg += suffix;

            _logger.Log(level, msg);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var prefix = LoggingProxyConfig.EnableColors ? ColorRed : "";
            var suffix = LoggingProxyConfig.EnableColors ? ColorReset : "";
            _logger.LogError(ex, $"{prefix}[LOGGING-PROXY ❌] {name} failed after {stopwatch.ElapsedMilliseconds}ms{suffix}");
            throw;
        }
    }

    private static string SafeSerialize(object? value)
    {
        try { return JsonSerializer.Serialize(value); }
        catch { return "[Unserializable]"; }
    }
}