using Microsoft.Extensions.Logging;
using System;

namespace Momkay.LoggingProxy.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LogAttribute : Attribute
{
    public LogLevel Level { get; }
    public LogAttribute(LogLevel level = LogLevel.Information) => Level = level;
}