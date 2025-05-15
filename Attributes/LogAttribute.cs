using Microsoft.Extensions.Logging;

namespace KlusFlow.LoggingProxy.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LogAttribute : Attribute
{
    public LogLevel Level { get; }
    public LogAttribute(LogLevel level = LogLevel.Information) => Level = level;
}