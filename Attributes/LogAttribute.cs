using Microsoft.Extensions.Logging;
using System;

namespace Momkay.LoggingProxy.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LogAttribute(LogLevel level = LogLevel.Debug) : Attribute
{
    public LogLevel Level { get; } = level;
}