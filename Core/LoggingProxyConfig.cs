using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Momkay.LoggingProxy.Core;

public static class LoggingProxyConfig
{
    public static bool EnableColors { get; set; } = true;
    public static bool LogReturnValuesByDefault { get; set; } = false;
    public static LogLevel DefaultLogLevel { get; set; } = LogLevel.Information;
    public static Func<MethodInfo, bool>? GlobalFilter { get; set; } = null;
}