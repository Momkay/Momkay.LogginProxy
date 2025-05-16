# Momkay.LoggingProxy

A lightweight proxy-based logging interceptor for .NET service interfaces. It automatically logs method calls, parameters, execution time, and exceptions — without cluttering your service code.

---

## Features

- Logs method entry, arguments, and exit duration
- Logs exceptions with full context
- Attribute-based control:
  - `[NoLog]` — skip logging
  - `[Log(LogLevel.Debug)]` — override log level per method
- Compatible with any DI container using interfaces
- No external dependencies or runtime reflection libraries

---

## Installation

Add the NuGet package to your project:

```bash
dotnet add package Momkay.LoggingProxy
```

## Setup

In your Program.cs (or wherever you configure services):

```Program.cs
using Momkay.LoggingProxy.Core;

builder.Services.AddLoggedServices(typeof(IMyService).Assembly);
```
This scans all types in the specified assembly and wraps matching service interfaces in logging proxies.

A service will be automatically wrapped if:

- It has an interface named `I[ClassName]`
- It is registered in DI via that interface

---

## Usage example

### Interface

```IMyService.cs
public interface IMyService
{
    Task DoWorkAsync();
    Task InternalHelper();
}
```

### Implementation

```MyService.cs
public class MyService : IMyService
{
    [Log(LogLevel.Debug)]
    public async Task DoWorkAsync()
    {
        // This method call will be logged with Debug level
    }

    [NoLog]
    public Task InternalHelper()
    {
        // This method will not be logged
        return Task.CompletedTask;
    }
}
```

### Attributes

You can fine-tune logging behavior using these attributes:

- `LogAttribute` Sets the log level for a specific method
- `NoLogAttribute`	Disables logging entirely for the method

These attributes are optional. By default, all public interface methods are logged at Information level.

---

## Requirements

- .NET 6 or later or .NET Standard 2.1 (for libraries)
- Interface-based service registration (e.g. IService, Service)
- Microsoft.Extensions.Logging in your app (standard in ASP.NET Core)

---
