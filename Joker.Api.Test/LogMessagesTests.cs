using Microsoft.Extensions.Logging;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="LogMessages"/> logging extension methods
/// </summary>
public class LogMessagesTests
{
	private sealed class RecordingLogger : ILogger
	{
		public IDisposable BeginScope<TState>(TState state) where TState : notnull
			=> NullScope.Instance;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
		}

		private sealed class NullScope : IDisposable
		{
			public static readonly NullScope Instance = new();

			public void Dispose()
			{
			}
		}
	}

	[Fact]
	public void LogDmapiRequest_DoesNotThrow()
	{
		// Arrange
		ILogger logger = new RecordingLogger();

		// Act & Assert (should not throw)
		logger.LogDmapiRequest("GET", "https://example.com/request/login");
	}

	[Fact]
	public void LogDmapiResponse_DoesNotThrow()
	{
		// Arrange
		ILogger logger = new RecordingLogger();

		// Act & Assert (should not throw)
		logger.LogDmapiResponse("result: ACK");
	}

	[Fact]
	public void LogSvcAuthenticated_DoesNotThrow()
	{
		// Arrange
		ILogger logger = new RecordingLogger();

		// Act & Assert (should not throw)
		logger.LogSvcAuthenticated("example.com");
	}

	[Fact]
	public void LogDnsZoneRetrieved_DoesNotThrow()
	{
		// Arrange
		ILogger logger = new RecordingLogger();

		// Act & Assert (should not throw)
		logger.LogDnsZoneRetrieved("example.com");
	}

	[Fact]
	public void LogDnsZoneUpdated_DoesNotThrow()
	{
		// Arrange
		ILogger logger = new RecordingLogger();

		// Act & Assert (should not throw)
		logger.LogDnsZoneUpdated("example.com", 42);
	}

	[Fact]
	public void LogSvcDmapiRequest_DoesNotThrow()
	{
		// Arrange
		ILogger logger = new RecordingLogger();

		// Act & Assert (should not throw)
		logger.LogSvcDmapiRequest("GET", "https://example.com/request/dns-zone-get");
	}

	[Fact]
	public void LogSvcDmapiResponse_DoesNotThrow()
	{
		// Arrange
		ILogger logger = new RecordingLogger();

		// Act & Assert (should not throw)
		logger.LogSvcDmapiResponse("A:@:1.2.3.4");
	}
}
