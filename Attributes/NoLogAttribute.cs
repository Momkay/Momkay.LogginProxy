using System;

namespace Momkay.LoggingProxy.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class NoLogAttribute : Attribute { }