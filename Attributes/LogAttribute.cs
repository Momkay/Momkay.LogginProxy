using Microsoft.Extensions.Logging;
using System;

namespace Momkay.LoggingProxy.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LogAttribute(LogLevel level = LogLevel.Information, bool includeReturnValue = false) : Attribute
{
    public LogLevel Level { get; } = level;
    public bool IncludeReturnValue { get; } = includeReturnValue;
}
