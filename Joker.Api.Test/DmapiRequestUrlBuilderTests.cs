using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="DmapiRequestUrlBuilder"/>
/// </summary>
public class DmapiRequestUrlBuilderTests
{
	[Fact]
	public void Build_WithNoParameters_ReturnsPlainRequestPath()
	{
		// Act
		var url = DmapiRequestUrlBuilder.Build("login", new Dictionary<string, string>());

		// Assert
		url.Should().Be("/request/login");
	}

	[Fact]
	public void Build_WithParameters_AppendsQueryString()
	{
		// Arrange
		var parameters = new Dictionary<string, string>
		{
			["username"] = "user@example.com",
			["password"] = "s3cret"
		};

		// Act
		var url = DmapiRequestUrlBuilder.Build("login", parameters);

		// Assert
		url.Should().StartWith("/request/login?");
		url.Should().Contain("username=user%40example.com");
		url.Should().Contain("password=s3cret");
	}

	[Fact]
	public void Build_WithSpecialCharacters_EscapesThem()
	{
		// Arrange
		var parameters = new Dictionary<string, string>
		{
			["pattern"] = "* a & b"
		};

		// Act
		var url = DmapiRequestUrlBuilder.Build("query-domain-list", parameters);

		// Assert
		url.Should().Be("/request/query-domain-list?pattern=%2A%20a%20%26%20b");
	}
}
