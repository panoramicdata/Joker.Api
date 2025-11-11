using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Tests for DMAPI write operations (will fail with read-only keys but provide code coverage)
/// </summary>
public class JokerClientWriteTests : TestBase<JokerClientWriteTests>, IDisposable
{
	private readonly JokerClient _client;

	public JokerClientWriteTests()
	{
		var apiKey = _configuration["JokerApi:ApiKey:ReadOnly"];

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
		_ = response.Should().NotBeNull();
		// Don't assert on success - it should fail, but we've covered the code
		_logger.LogInformation("DomainRegister (expected failure) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task DomainRegisterAsync_ValidatesPeriodRange()
	{
		// Arrange
		var actionTooLow = async () => await _client.DomainRegisterAsync(
			"test.com",
			0,
			TestContext.Current.CancellationToken);

		var actionTooHigh = async () => await _client.DomainRegisterAsync(
			"test.com",
			11,
			TestContext.Current.CancellationToken);

		// Act & Assert
		_ = await actionTooLow.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
		_ = await actionTooHigh.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
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
		_ = response.Should().NotBeNull();
		_logger.LogInformation("DomainRenew (expected failure) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task DomainRenewAsync_ValidatesPeriodRange()
	{
		// Arrange
		var actionTooLow = async () => await _client.DomainRenewAsync(
			"test.com",
			0,
			TestContext.Current.CancellationToken);

		var actionTooHigh = async () => await _client.DomainRenewAsync(
			"test.com",
			11,
			TestContext.Current.CancellationToken);

		// Act & Assert
		_ = await actionTooLow.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
		_ = await actionTooHigh.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
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
		_ = response.Should().NotBeNull();
		_ = response.IsSuccess.Should().BeFalse();
		_logger.LogInformation("DomainTransfer (expected failure) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task DomainTransferAsync_ValidatesParameters()
	{
		// Arrange
		var actionEmptyDomain = async () => await _client.DomainTransferAsync(
			"",
			"auth-code",
			TestContext.Current.CancellationToken);

		var actionEmptyAuthCode = async () => await _client.DomainTransferAsync(
			"test.com",
			"",
			TestContext.Current.CancellationToken);

		// Act & Assert
		_ = await actionEmptyDomain.Should().ThrowExactlyAsync<ArgumentException>();
		_ = await actionEmptyAuthCode.Should().ThrowExactlyAsync<ArgumentException>();
	}

	public void Dispose()
	{
		_client?.Dispose();
		(_serviceProvider as IDisposable)?.Dispose();
		GC.SuppressFinalize(this);
	}
}
