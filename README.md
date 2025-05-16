# Momkay.LoggingProxy

A lightweight proxy-based logging interceptor for .NET service interfaces. mIt automatically logs method calls, parameters, execution time, and exceptions — without cluttering your service code.

---

## Features

- Logs method entry, arguments, and execution duration
- Logs exceptions with full stack trace
- Works with **sync** and **async** (`Task`, `Task<T>`) methods
- Attribute-based control:
  - `[NoLog]` — skip logging
  - `[Log(LogLevel.Debug)]` — override log level
- Console log coloring (optional, terminal-dependent)
- No external dependencies
- Zero boilerplate with `AddLoggedServices(...)`

---

## Installation

```bash
dotnet add package Momkay.LoggingProxy
```

---

## Setup

In your `Program.cs` (or startup configuration):

```csharp
using Momkay.LoggingProxy.Core;

builder.Services.AddLoggedServices(typeof(IMyService).Assembly);
```

This will:
- Scan all classes in the given assembly
- Match them with interfaces named `I[ClassName]`
- Register them automatically with `Scoped` lifetime
- Wrap them in a proxy that injects structured logging

> ⚠️ You do **not** need to call `AddScoped<IMyService, MyService>()` manually.

---

## Usage Example

### Interface

```csharp
public interface IMyService
{
    Task DoWorkAsync();
    Task InternalHelper();
}
```

### Implementation

```csharp
public class MyService : IMyService
{
    [Log(LogLevel.Debug)]
    public async Task DoWorkAsync()
    {
        // This will be logged at Debug level
    }

    [NoLog]
    public Task InternalHelper()
    {
        // This will be skipped in logging
        return Task.CompletedTask;
    }
}
```

---

## Attributes

| Attribute     | Description                             |
|---------------|-----------------------------------------|
| `[NoLog]`     | Excludes method from logging            |
| `[Log(Level)]`| Overrides log level (default: Info)     |

---

## Return Value Logging

You can log method return values by setting:

Per method:
```csharp
[Log(includeReturnValue: true)]
public Task<string> GetTokenAsync() { ... }
```

Globally:
```csharp
LoggingProxyConfig.LogReturnValuesByDefault = true;
```

---

## Console Output

By default, logs are written via `ILogger<T>`.  
For better readability, the proxy includes optional ANSI colors (cyan/green/red) for:

- Entry `▶`
- Success `✔`
- Errors `❌`

Colors only appear if your console supports ANSI (e.g. VS Code terminal, Windows Terminal, bash).

To disable colors globally:

```bash
set LOGGING_PROXY_COLORS=0
```

---

## Requirements

- .NET 6 or later (or .NET Standard 2.1 for library use)
- Interface-based registration pattern (e.g. `IFooService` / `FooService`)
- `Microsoft.Extensions.Logging` (already standard in ASP.NET Core)

---

## License

MIT — Use it, extend it, improve it. Contributions welcome!