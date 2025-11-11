using AwesomeAssertions;

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
		_ = options.Domain.Should().Be("example.com");
		_ = options.SvcUsername.Should().Be("svc-user");
		_ = options.SvcPassword.Should().Be("svc-pass");
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
		_ = options.BaseUrl.Should().Be("https://dmapi.joker.com");
		Assert.Equal(TimeSpan.FromSeconds(30), options.RequestTimeout);
		_ = options.EnableRequestLogging.Should().BeFalse();
		_ = options.EnableResponseLogging.Should().BeFalse();
		_ = options.Logger.Should().BeNull();
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
		_ = options.BaseUrl.Should().Be("https://custom.api.url");
		Assert.Equal(TimeSpan.FromMinutes(2), options.RequestTimeout);
		_ = options.EnableRequestLogging.Should().BeTrue();
		_ = options.EnableResponseLogging.Should().BeTrue();
	}
}
