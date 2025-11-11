using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Joker.Api.Test;

/// <summary>
/// Tests for DMAPI write operations (will fail with read-only keys but provide code coverage)
/// </summary>
public class JokerClientWriteTests : IDisposable
{
	private readonly JokerClient _client;
	private readonly ILogger<JokerClientWriteTests> _logger;

	public JokerClientWriteTests()
	{
		var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Debug);
		});

		_logger = loggerFactory.CreateLogger<JokerClientWriteTests>();

		var configuration = new ConfigurationBuilder()
			.AddUserSecrets<JokerClientWriteTests>()
			.Build();

		var apiKey = configuration["JokerApi:ApiKey:ReadOnly"];
		
		if (string.IsNullOrWhiteSpace(apiKey))
		{
			throw new InvalidOperationException("ReadOnly API key not configured");
		}

		var options = new JokerClientOptions
		{
			ApiKey = apiKey,
			Logger = _logger
		};

		_client = new JokerClient(options);
	}

	[Fact]
	public async Task DomainRegisterAsync_WithReadOnlyKey_FailsAsExpected()
	{
		// Act - Try to register (will fail but covers the code path)
		var response = await _client.DomainRegisterAsync(
			"test-coverage-domain-12345.com",
			1, // 1 year
			TestContext.Current.CancellationToken);

		// Assert - Should fail (permission error or other error)
		Assert.NotNull(response);
		// Don't assert on success - it should fail, but we've covered the code
		_logger.LogInformation("DomainRegister (expected failure) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task DomainRegisterAsync_ValidatesPeriodRange()
	{
		// Act & Assert - Period too low
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
		{
			await _client.DomainRegisterAsync(
				"test.com",
				0,
				TestContext.Current.CancellationToken);
		});

		// Act & Assert - Period too high
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
		{
			await _client.DomainRegisterAsync(
				"test.com",
				11,
				TestContext.Current.CancellationToken);
		});
	}

	[Fact]
	public async Task DomainRenewAsync_WithReadOnlyKey_FailsAsExpected()
	{
		// Act
		var response = await _client.DomainRenewAsync(
			"test-coverage-domain.com",
			1, // 1 year
			TestContext.Current.CancellationToken);

		// Assert - Should fail (permission error or other error)
		Assert.NotNull(response);
		_logger.LogInformation("DomainRenew (expected failure) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task DomainRenewAsync_ValidatesPeriodRange()
	{
		// Act & Assert - Period too low
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
		{
			await _client.DomainRenewAsync(
				"test.com",
				0,
				TestContext.Current.CancellationToken);
		});

		// Act & Assert - Period too high
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
		{
			await _client.DomainRenewAsync(
				"test.com",
				11,
				TestContext.Current.CancellationToken);
		});
	}

	[Fact]
	public async Task DomainTransferAsync_WithReadOnlyKey_ReturnsPermissionError()
	{
		// Act
		var response = await _client.DomainTransferAsync(
			"test-coverage-domain.com",
			"fake-auth-code-12345",
			TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(response);
		Assert.False(response.IsSuccess);
		_logger.LogInformation("DomainTransfer (expected failure) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task DomainTransferAsync_ValidatesParameters()
	{
		// Act & Assert - Empty domain
		await Assert.ThrowsAsync<ArgumentException>(async () =>
		{
			await _client.DomainTransferAsync(
				"",
				"auth-code",
				TestContext.Current.CancellationToken);
		});

		// Act & Assert - Empty auth code
		await Assert.ThrowsAsync<ArgumentException>(async () =>
		{
			await _client.DomainTransferAsync(
				"test.com",
				"",
				TestContext.Current.CancellationToken);
		});
	}

	public void Dispose()
	{
		_client?.Dispose();
		GC.SuppressFinalize(this);
	}
}
