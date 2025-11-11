namespace Joker.Api.Test;

/// <summary>
/// Tests for JokerSvcClientOptions configuration
/// </summary>
public class JokerSvcClientOptionsTests
{
	[Fact]
	public void Constructor_WithRequiredProperties_CreatesInstance()
	{
		// Arrange & Act
		var options = new JokerSvcClientOptions
		{
			Domain = "example.com",
			SvcUsername = "svc-user",
			SvcPassword = "svc-pass"
		};

		// Assert
		Assert.Equal("example.com", options.Domain);
		Assert.Equal("svc-user", options.SvcUsername);
		Assert.Equal("svc-pass", options.SvcPassword);
	}

	[Fact]
	public void DefaultValues_AreSetCorrectly()
	{
		// Arrange & Act
		var options = new JokerSvcClientOptions
		{
			Domain = "example.com",
			SvcUsername = "svc-user",
			SvcPassword = "svc-pass"
		};

		// Assert
		Assert.Equal("https://dmapi.joker.com", options.BaseUrl);
		Assert.Equal(TimeSpan.FromSeconds(30), options.RequestTimeout);
		Assert.False(options.EnableRequestLogging);
		Assert.False(options.EnableResponseLogging);
		Assert.Null(options.Logger);
	}

	[Fact]
	public void CustomValues_CanBeSet()
	{
		// Arrange & Act
		var options = new JokerSvcClientOptions
		{
			Domain = "example.com",
			SvcUsername = "svc-user",
			SvcPassword = "svc-pass",
			BaseUrl = "https://custom.api.url",
			RequestTimeout = TimeSpan.FromMinutes(2),
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		// Assert
		Assert.Equal("https://custom.api.url", options.BaseUrl);
		Assert.Equal(TimeSpan.FromMinutes(2), options.RequestTimeout);
		Assert.True(options.EnableRequestLogging);
		Assert.True(options.EnableResponseLogging);
	}
}
