using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="JokerClientBase"/> dispose pattern
/// </summary>
public class JokerClientBaseTests
{
	private sealed class RecordingClient : JokerClientBase
	{
		public int DisposeCalls { get; private set; }

		protected override void Dispose(bool disposing)
		{
			DisposeCalls++;
		}
	}

	[Fact]
	public void Dispose_CallsDisposeTrue_Once()
	{
		// Arrange
		var client = new RecordingClient();

		// Act
		client.Dispose();

		// Assert
		client.DisposeCalls.Should().Be(1);
	}

	[Fact]
	public void Dispose_SecondCall_DoesNotCallDisposeAgain()
	{
		// Arrange
		var client = new RecordingClient();
		client.Dispose();

		// Act
		client.Dispose();

		// Assert
		client.DisposeCalls.Should().Be(1);
	}
}
