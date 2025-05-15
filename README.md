# KlusFlow.LoggingProxy

A lightweight proxy-based logging interceptor for .NET service interfaces.  
It automatically logs method calls, arguments, execution time, and exceptions — without polluting your service code.

## Features

- Automatic method call logging (entry + exit)
- Logs method arguments and execution duration
- Exception logging with full context
- Attribute-based control per method:
  - `[NoLog]` — skip logging
  - `[Log(LogLevel.Debug)]` — override log level
- Simple DI integration: no boilerplate

## Installation

```bash
dotnet pack -c Release
dotnet add package KlusFlow.LoggingProxy --source ./bin/Release
