using Joker.Api;

namespace Joker.Api.Test;

public class JokerClientTests
{
	[Fact]
	public void Constructor_WithValidOptions_CreatesClient()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			ApiKey = "test-api-key"
		};

		// Act
		using var client = new JokerClient(options);

		// Assert
		Assert.NotNull(client);
	}

	[Fact]
	public void Constructor_WithNullOptions_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new JokerClient(null!));
	}
}
