using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Tests for JokerClientOptions validation and configuration
/// </summary>
public class JokerClientOptionsTests
{
	[Fact]
	public void Validate_WithApiKey_DoesNotThrow()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			ApiKey = "test-api-key"
		};

		// Act & Assert (should not throw)
		options.Validate();
	}

	[Fact]
	public void Validate_WithUsernameAndPassword_DoesNotThrow()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			Username = "test@example.com",
			Password = "password123"
		};

		// Act & Assert (should not throw)
		options.Validate();
	}

	[Fact]
	public void Validate_WithNoCredentials_ThrowsInvalidOperationException()
	{
		// Arrange
		var options = new JokerClientOptions();

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
		Assert.Contains("ApiKey or both Username and Password", exception.Message);
	}

	[Fact]
	public void Validate_WithOnlyUsername_ThrowsInvalidOperationException()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			Username = "test@example.com"
		};

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
		Assert.Contains("ApiKey or both Username and Password", exception.Message);
	}

	[Fact]
	public void Validate_WithOnlyPassword_ThrowsInvalidOperationException()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			Password = "password123"
		};

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
		Assert.Contains("ApiKey or both Username and Password", exception.Message);
	}

	[Fact]
	public void DefaultValues_AreSetCorrectly()
	{
		// Arrange & Act
		var options = new JokerClientOptions
		{
			ApiKey = "test-key"
		};

		// Assert
		options.BaseUrl.Should().Be("https://dmapi.joker.com");
		options.RequestTimeout.Should().Be(TimeSpan.FromSeconds(30));
		options.MaxRetryAttempts.Should().Be(3);
		options.RetryDelay.Should().Be(TimeSpan.FromSeconds(1));
		options.UseExponentialBackoff.Should().BeTrue();
		options.MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(30));
		options.EnableRequestLogging.Should().BeFalse();
		options.EnableResponseLogging.Should().BeFalse();
		options.Logger.Should().BeNull();
	}

	[Fact]
	public void CustomValues_CanBeSet()
	{
		// Arrange & Act
		var options = new JokerClientOptions
		{
			ApiKey = "custom-key",
			BaseUrl = "https://custom.api.url",
			RequestTimeout = TimeSpan.FromMinutes(2),
			MaxRetryAttempts = 5,
			RetryDelay = TimeSpan.FromSeconds(5),
			UseExponentialBackoff = false,
			MaxRetryDelay = TimeSpan.FromMinutes(1),
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		// Assert
		options.ApiKey.Should().Be("custom-key");
		options.BaseUrl.Should().Be("https://custom.api.url");
		options.RequestTimeout.Should().Be(TimeSpan.FromMinutes(2));
		options.MaxRetryAttempts.Should().Be(5);
		options.RetryDelay.Should().Be(TimeSpan.FromSeconds(5));
		options.UseExponentialBackoff.Should().BeFalse();
		options.MaxRetryDelay.Should().Be(TimeSpan.FromMinutes(1));
		options.EnableRequestLogging.Should().BeTrue();
		options.EnableResponseLogging.Should().BeTrue();
	}
}
