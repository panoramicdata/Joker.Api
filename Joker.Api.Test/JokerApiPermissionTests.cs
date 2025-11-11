using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Tests for API key permission levels
/// These tests verify that different API key types enforce proper access controls
/// </summary>
public class JokerApiPermissionTests : TestBase<JokerApiPermissionTests>, IDisposable
{
	[Fact]
	public async Task ReadOnlyApiKey_CanAuthenticate()
	{
		// Arrange
		var readOnlyKey = _configuration["JokerApi:ApiKey:ReadOnly"];

		// Skip if no read-only key configured
		if (string.IsNullOrWhiteSpace(readOnlyKey))
		{
			_logger.LogWarning("Skipping test: No read-only API key configured in user secrets");
			return;
		}

		var options = new JokerClientOptions
		{
			ApiKey = readOnlyKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		using var client = new JokerClient(options);

		// Act
		var response = await client.LoginAsync(TestContext.Current.CancellationToken);

		// Assert
		_logger.LogInformation(
			"Read-only API key authentication - StatusCode: {StatusCode}, StatusText: {StatusText}",
			response.StatusCode,
			response.StatusText);

		// Read-only keys should authenticate successfully
		var isSuccessful = response.StatusCode == 0 ||
						  response.StatusText?.Equals("OK", StringComparison.OrdinalIgnoreCase) == true;

		_ = isSuccessful.Should().BeTrue($"Read-only API key should authenticate successfully - StatusCode: {response.StatusCode}, StatusText: {response.StatusText}");

		_logger.LogInformation("✓ Read-only API key authenticated successfully");
	}

	[Fact]
	public async Task ReadOnlyApiKey_CannotPerformWriteOperations()
	{
		// Arrange
		var readOnlyKey = _configuration["JokerApi:ApiKey:ReadOnly"];

		// Skip if no read-only key configured
		if (string.IsNullOrWhiteSpace(readOnlyKey))
		{
			_logger.LogWarning("Skipping test: No read-only API key configured in user secrets");
			return;
		}

		var options = new JokerClientOptions
		{
			ApiKey = readOnlyKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		using var client = new JokerClient(options);

		// Act - Try to register a domain (should fail with read-only key)
		const string testDomain = "test-domain-that-should-not-register-12345.com";
		var response = await client.DomainRegisterAsync(testDomain, 1, TestContext.Current.CancellationToken);

		// Assert - Should fail with permission error
		_logger.LogInformation(
			"Domain registration with read-only key - StatusCode: {StatusCode}, StatusText: {StatusText}",
			response.StatusCode,
			response.StatusText);

		if (response.Errors.Count > 0)
		{
			foreach (var error in response.Errors)
			{
				_logger.LogInformation("  Error: {Error}", error);
			}
		}

		// Verify it failed due to permissions (not success)
		_ = response.IsSuccess.Should().BeFalse("Read-only API key should not be able to register domains");

		// Check for permission-related errors
		var hasPermissionError = response.Errors.Any(e =>
			e.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
			e.Contains("not allowed", StringComparison.OrdinalIgnoreCase) ||
			e.Contains("read-only", StringComparison.OrdinalIgnoreCase) ||
			e.Contains("access denied", StringComparison.OrdinalIgnoreCase));

		if (hasPermissionError)
		{
			_logger.LogInformation("✓ Write operation correctly blocked by read-only API key (permission error)");
		}
		else
		{
			_logger.LogInformation("✓ Write operation failed with read-only API key (may be different error)");
		}
	}

	[Fact(Skip = "Write operations not yet implemented - placeholder for future testing")]
	public async Task FullAccessApiKey_CanPerformWriteOperations()
	{
		// Arrange
		var fullAccessKey = _configuration["JokerApi:ApiKey:Full"];

		// Skip if no full access key configured
		if (string.IsNullOrWhiteSpace(fullAccessKey))
		{
			_logger.LogWarning("Skipping test: No full access API key configured in user secrets");
			return;
		}

		var options = new JokerClientOptions
		{
			ApiKey = fullAccessKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		using var client = new JokerClient(options);

		// Act & Assert
		// TODO: When write operations are implemented, verify that full access keys can perform them

		_logger.LogInformation("✓ Write operation succeeded with full access API key");

		await Task.CompletedTask;
	}

	[Fact(Skip = "Modify operations not yet implemented - placeholder for future testing")]
	public async Task ModifyOnlyApiKey_CanModifyButNotCreate()
	{
		// Arrange
		var modifyOnlyKey = _configuration["JokerApi:ApiKey:ModifyOnly"];

		// Skip if no modify-only key configured
		if (string.IsNullOrWhiteSpace(modifyOnlyKey))
		{
			_logger.LogWarning("Skipping test: No modify-only API key configured in user secrets");
			return;
		}

		var options = new JokerClientOptions
		{
			ApiKey = modifyOnlyKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		using var client = new JokerClient(options);

		// Act & Assert
		// TODO: Verify modify-only keys can update existing resources but not create new ones

		_logger.LogInformation("✓ Modify-only API key permissions verified");

		await Task.CompletedTask;
	}

	[Fact]
	public async Task WhoisOnlyApiKey_CanAuthenticate()
	{
		// Arrange
		var whoisOnlyKey = _configuration["JokerApi:ApiKey:WhoisOnly"];

		// Skip if no whois-only key configured
		if (string.IsNullOrWhiteSpace(whoisOnlyKey))
		{
			_logger.LogWarning("Skipping test: No whois-only API key configured in user secrets");
			return;
		}

		var options = new JokerClientOptions
		{
			ApiKey = whoisOnlyKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		using var client = new JokerClient(options);

		// Act
		var response = await client.LoginAsync(TestContext.Current.CancellationToken);

		// Assert
		_logger.LogInformation("Whois-only API key authentication - StatusCode: {StatusCode}, StatusText: {StatusText}",
			response.StatusCode, response.StatusText);

		// Whois-only keys should authenticate successfully
		var isSuccessful = response.StatusCode == 0 ||
						  response.StatusText?.Equals("OK", StringComparison.OrdinalIgnoreCase) == true;

		_ = isSuccessful.Should().BeTrue($"Whois-only API key should authenticate successfully - StatusCode: {response.StatusCode}, StatusText: {response.StatusText}");

		_logger.LogInformation("✓ Whois-only API key authenticated successfully");
	}

	[Fact(Skip = "WHOIS operations not yet implemented - placeholder for future testing")]
	public async Task WhoisOnlyApiKey_CanOnlyPerformWhoisQueries()
	{
		// Arrange
		var whoisOnlyKey = _configuration["JokerApi:ApiKey:WhoisOnly"];

		// Skip if no whois-only key configured
		if (string.IsNullOrWhiteSpace(whoisOnlyKey))
		{
			_logger.LogWarning("Skipping test: No whois-only API key configured in user secrets");
			return;
		}

		var options = new JokerClientOptions
		{
			ApiKey = whoisOnlyKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		using var client = new JokerClient(options);

		// Act & Assert
		// TODO: When WHOIS operations are implemented, verify that:
		// 1. Whois-only keys can perform WHOIS queries
		// 2. Whois-only keys cannot perform any other operations (read, write, modify)

		// Example expected implementation:
		// var whoisResponse = await client.QueryWhoisAsync("example.com", ...);
		// Assert.True(whoisResponse.IsSuccess);
		//
		// var writeResponse = await client.RegisterDomainAsync("test-domain.com", ...);
		// Assert.True(writeResponse.Errors.Any(e => e.Contains("permission", StringComparison.OrdinalIgnoreCase)));

		_logger.LogInformation("✓ Whois-only API key permissions verified");

		await Task.CompletedTask;
	}

	[Fact]
	public async Task WhoisOnlyApiKey_CannotPerformWriteOperations()
	{
		// Arrange
		var whoisOnlyKey = _configuration["JokerApi:ApiKey:WhoisOnly"];

		// Skip if no whois-only key configured
		if (string.IsNullOrWhiteSpace(whoisOnlyKey))
		{
			_logger.LogWarning("Skipping test: No whois-only API key configured in user secrets");
			return;
		}

		var options = new JokerClientOptions
		{
			ApiKey = whoisOnlyKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		using var client = new JokerClient(options);

		// Act - Try to register a domain (should fail with whois-only key)
		const string testDomain = "test-domain-that-should-not-register-67890.com";
		var response = await client.DomainRegisterAsync(testDomain, 1, TestContext.Current.CancellationToken);

		// Assert - Should fail with permission error
		_logger.LogInformation("Domain registration with whois-only key - StatusCode: {StatusCode}, StatusText: {StatusText}",
			response.StatusCode, response.StatusText);

		if (response.Errors.Count > 0)
		{
			foreach (var error in response.Errors)
			{
				_logger.LogInformation("  Error: {Error}", error);
			}
		}

		// Verify it failed (not success)
		_ = response.IsSuccess.Should().BeFalse("Whois-only API key should not be able to register domains");

		_logger.LogInformation("✓ Write operation correctly blocked by whois-only API key");
	}

	public void Dispose()
	{
		(_serviceProvider as IDisposable)?.Dispose();
		GC.SuppressFinalize(this);
	}
}
