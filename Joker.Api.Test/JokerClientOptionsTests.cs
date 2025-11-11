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
		Assert.Equal("https://dmapi.joker.com", options.BaseUrl);
		Assert.Equal(TimeSpan.FromSeconds(30), options.RequestTimeout);
		Assert.Equal(3, options.MaxRetryAttempts);
		Assert.Equal(TimeSpan.FromSeconds(1), options.RetryDelay);
		Assert.True(options.UseExponentialBackoff);
		Assert.Equal(TimeSpan.FromSeconds(30), options.MaxRetryDelay);
		Assert.False(options.EnableRequestLogging);
		Assert.False(options.EnableResponseLogging);
		Assert.Null(options.Logger);
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
		Assert.Equal("custom-key", options.ApiKey);
		Assert.Equal("https://custom.api.url", options.BaseUrl);
		Assert.Equal(TimeSpan.FromMinutes(2), options.RequestTimeout);
		Assert.Equal(5, options.MaxRetryAttempts);
		Assert.Equal(TimeSpan.FromSeconds(5), options.RetryDelay);
		Assert.False(options.UseExponentialBackoff);
		Assert.Equal(TimeSpan.FromMinutes(1), options.MaxRetryDelay);
		Assert.True(options.EnableRequestLogging);
		Assert.True(options.EnableResponseLogging);
	}
}
