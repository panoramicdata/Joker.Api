# Joker DMAPI .NET Library

[![NuGet Version](https://img.shields.io/nuget/v/Joker.Api)](https://www.nuget.org/packages/Joker.Api)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Joker.Api)](https://www.nuget.org/packages/Joker.Api)
[![Build Status](https://img.shields.io/github/actions/workflow/status/panoramicdata/Joker.Api/publish-nuget.yml)](https://github.com/panoramicdata/Joker.Api/actions)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/c790c2232f2745eca8b5b47a145e37b9)](https://app.codacy.com/gh/panoramicdata/Joker.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive, modern .NET library for interacting with the Joker DMAPI API. This library provides full coverage of the Joker DMAPI API with a clean, intuitive interface using modern C# patterns and best practices.

## Features

- 🎯 **Complete API Coverage** - Full support for all Joker DMAPI endpoints
- 🚀 **Modern .NET** - Built for .NET 9 with modern C# features
- 🔒 **Type Safety** - Strongly typed models and responses
- 📝 **Comprehensive Logging** - Built-in logging and request/response interception
- 🔄 **Retry Logic** - Automatic retry with exponential backoff
- 📖 **Rich Documentation** - IntelliSense-friendly XML documentation
- ✅ **Thoroughly Tested** - Comprehensive unit and integration tests
- ⚡ **High Performance** - Optimized for efficiency and low memory usage

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Joker.Api
```

Or via Package Manager Console:

```powershell
Install-Package Joker.Api
```

## Quick Start

### 1. Authentication Setup

The Joker DMAPI supports two authentication methods:

#### Option 1: API Key (Recommended for automated access)

```csharp
using Joker.Api;

var options = new JokerClientOptions
{
    ApiKey = "your-api-key"
};

var client = new JokerClient(options);
```

#### Option 2: Username and Password

```csharp
using Joker.Api;

var options = new JokerClientOptions
{
    Username = "user@joker.com",
    Password = "your-password"
};

var client = new JokerClient(options);
```

### 2. Basic Usage Examples

```csharp
// Use a CancellationToken for all async operations
using var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

// Example API calls will be added as the library is developed
```

### 3. Advanced Configuration

#### Custom HTTP Configuration

```csharp
var options = new JokerClientOptions
{
    ApiKey = "your-api-key",
    
    // Custom timeout
    RequestTimeout = TimeSpan.FromSeconds(30),
    
    // Custom retry policy
    MaxRetryAttempts = 3,
    RetryDelay = TimeSpan.FromSeconds(1)
};

var client = new JokerClient(options);
```

#### Logging Configuration

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Create a service collection with logging
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<JokerClient>>();

var options = new JokerClientOptions
{
    ApiKey = "your-api-key",
    Logger = logger,
    EnableRequestLogging = true,
    EnableResponseLogging = true
};

var client = new JokerClient(options);
```

### 4. Error Handling

```csharp
try
{
    // API call example
}
catch (JokerNotFoundException ex)
{
    Console.WriteLine($"Resource not found: {ex.Message}");
}
catch (JokerAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (JokerApiException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
    Console.WriteLine($"Status code: {ex.StatusCode}");
}
```

## API Coverage

This library provides comprehensive coverage of the Joker DMAPI API. Documentation will be expanded as endpoints are implemented.

## Configuration Options

The `JokerClientOptions` class provides extensive configuration:

```csharp
public class JokerClientOptions
{
    // Authentication (choose one method)
    public string? ApiKey { get; init; }              // Option 1: API Key (recommended)
    public string? Username { get; init; }            // Option 2: Username
    public string? Password { get; init; }            // Option 2: Password
    
    // Optional configuration
    public string BaseUrl { get; init; } = "https://dmapi.joker.com";
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public int MaxRetryAttempts { get; init; } = 3;
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(1);
    public ILogger? Logger { get; init; } = null;
    
    // Advanced options
    public bool EnableRequestLogging { get; init; } = false;
    public bool EnableResponseLogging { get; init; } = false;
    public bool UseExponentialBackoff { get; init; } = true;
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(30);
}
```

## Contributing

We welcome contributions from the community! Here's how you can help:

### Development Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/panoramicdata/Joker.Api.git
   cd Joker.Api
   ```

2. **Install .NET 9 SDK**:
   Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

3. **Set up User Secrets for testing**:
   
   Using API Key (recommended):
   ```bash
   cd Joker.Api.Test
   dotnet user-secrets init
   dotnet user-secrets set "JokerApi:ApiKey" "your-api-key"
   ```
   
   Or using Username/Password:
   ```bash
   cd Joker.Api.Test
   dotnet user-secrets init
   dotnet user-secrets set "JokerApi:Username" "user@joker.com"
   dotnet user-secrets set "JokerApi:Password" "your-password"
   ```

4. **Build and test**:
   ```bash
   dotnet build
   dotnet test
   ```

### Coding Standards

- **Follow the project's coding standards** as defined in `.editorconfig`
- **Use modern C# patterns** (primary constructors, collection expressions, etc.)
- **Maintain zero warnings policy** - all code must compile without warnings
- **Write comprehensive tests** - both unit and integration tests
- **Document public APIs** - use XML documentation comments

### Pull Request Process

1. **Fork the repository** and create a feature branch
2. **Write tests** for all new functionality
3. **Ensure all tests pass** including integration tests
4. **Update documentation** as needed
5. **Submit a pull request** with a clear description of changes

## Support

- **GitHub Issues**: [Report Issues](https://github.com/panoramicdata/Joker.Api/issues)
- **GitHub Discussions**: [Community Support](https://github.com/panoramicdata/Joker.Api/discussions)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Copyright

Copyright © 2025 Panoramic Data Limited. All rights reserved.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes and releases.
