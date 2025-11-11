# Joker DMAPI .NET Library

[![NuGet Version](https://img.shields.io/nuget/v/Joker.Api)](https://www.nuget.org/packages/Joker.Api)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Joker.Api)](https://www.nuget.org/packages/Joker.Api)
[![Build Status](https://img.shields.io/github/actions/workflow/status/panoramicdata/Joker.Api/publish-nuget.yml)](https://github.com/panoramicdata/Joker.Api/actions)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/c790c2232f2745eca8b5b47a145e37b9)](https://app.codacy.com/gh/panoramicdata/Joker.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive, modern .NET library for interacting with the Joker DMAPI API. This library provides full coverage of the Joker DMAPI API with a clean, intuitive interface using modern C# patterns and best practices.

## Features

- 🎯 **Complete API Coverage** - Full support for all Joker DMAPI endpoints
- 🔓 **SVC Support** - DNS management without reseller account (Dynamic DNS)
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

### Authentication Methods

Joker.Api supports multiple authentication methods:

1. **SVC (Dynamic DNS)** - For DNS management without a reseller account
2. **DMAPI with API Key** - Recommended for reseller accounts
3. **DMAPI with Username/Password** - Alternative for reseller accounts

#### API Key Permission Levels

Joker.com supports different API key types with varying permission levels:

- **Full Access** - Complete read and write access to all API operations
- **Read-Only** - Can only query and retrieve information, cannot make modifications
- **Modify-Only** - Can modify existing resources but cannot create new ones
- **Whois-Only** - Can only perform WHOIS queries, no other operations allowed

When creating API keys in your Joker.com reseller dashboard, select the appropriate permission level based on your security requirements. For automated systems that only need to query information (e.g., monitoring tools), use read-only or whois-only keys to minimize security risk.

### 1. SVC Client (DNS Management - No Reseller Account Required)

Perfect for automating DNS updates, ACME DNS-01 challenges (Let's Encrypt), and managing DNS records.

```csharp
using Joker.Api;
using Joker.Api.Models;

// Get SVC credentials from Joker.com dashboard:
// 1. Go to DNS settings for your domain
// 2. Enable Dynamic DNS
// 3. Copy the shown username and password

var options = new JokerSvcClientOptions
{
    Domain = "yourdomain.com",
    SvcUsername = "svc-username-from-dashboard",
    SvcPassword = "svc-password-from-dashboard"
};

using var client = new JokerSvcClient(options);

// Add a TXT record (e.g., for Let's Encrypt DNS-01 challenge)
await client.SetTxtRecordAsync(
    "_acme-challenge", 
    "verification-token-here",
    ttl: 300,
    cancellationToken);

// Get current DNS zone
var zone = await client.GetDnsZoneAsync(cancellationToken);
Console.WriteLine($"Current DNS zone:\n{zone.Body}");

// Delete a TXT record
await client.DeleteTxtRecordAsync("_acme-challenge", cancellationToken);

// Set multiple DNS records at once
var records = new[]
{
    DnsRecord.CreateARecord("www", "123.45.67.89"),
    DnsRecord.CreateCnameRecord("blog", "blog.provider.com"),
    DnsRecord.CreateTxtRecord("@", "v=spf1 include:_spf.provider.com ~all")
};
await client.SetDnsZoneAsync(records, cancellationToken);
```

### 2. DMAPI Client (Reseller Account Required)

#### Option 1: API Key (Recommended for reseller automation)

```csharp
using Joker.Api;

var options = new JokerClientOptions
{
    ApiKey = "your-api-key"
};

var client = new JokerClient(options);
```

#### Option 2: Username and Password (Reseller accounts)

```csharp
using Joker.Api;

var options = new JokerClientOptions
{
    Username = "user@joker.com",
    Password = "your-password"
};

var client = new JokerClient(options);
```

### 3. Basic Usage Examples

```csharp
// Use a CancellationToken for all async operations
using var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

// DMAPI operations (reseller account required)
var loginResponse = await client.LoginAsync(cancellationToken);
Console.WriteLine($"Logged in with session: {loginResponse.AuthSid}");
```

### 4. Advanced Configuration

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
   
   The test suite supports different API key permission levels. You can configure one or more:
   
   Using API Key (recommended):
   ```bash
   cd Joker.Api.Test
   dotnet user-secrets init
   # Default API key for most tests
   dotnet user-secrets set "JokerApi:ApiKey" "your-api-key"
   
   # Optional: Configure specific permission levels for permission testing
   dotnet user-secrets set "JokerApi:ApiKey:Full" "your-full-access-key"
   dotnet user-secrets set "JokerApi:ApiKey:ReadOnly" "your-read-only-key"
   dotnet user-secrets set "JokerApi:ApiKey:ModifyOnly" "your-modify-only-key"
   dotnet user-secrets set "JokerApi:ApiKey:WhoisOnly" "your-whois-only-key"
   ```
   
   Or using Username/Password:
   ```bash
   cd Joker.Api.Test
   dotnet user-secrets init
   dotnet user-secrets set "JokerApi:Username" "user@joker.com"
   dotnet user-secrets set "JokerApi:Password" "your-password"
   ```
   
   For SVC (Dynamic DNS) testing:
   ```bash
   dotnet user-secrets set "JokerSvc:Domain" "yourdomain.com"
   dotnet user-secrets set "JokerSvc:SvcUsername" "svc-username"
   dotnet user-secrets set "JokerSvc:SvcPassword" "svc-password"
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

---

### Development

This project was created entirely at the command line using:
- **[GitHub Copilot CLI](https://githubnext.com/projects/copilot-cli)** - AI-powered command-line assistant
- **[Claude Sonnet 4.5](https://www.anthropic.com/claude)** - Advanced AI coding assistant

No traditional IDE was used in the development of this library. All code was written, tested, and debugged using AI-assisted command-line tools, demonstrating the power of modern AI development workflows.
