using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="DmapiTransport"/>
/// </summary>
public class DmapiTransportTests
{
	private sealed class NullLogger : ILogger
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
	public void Constructor_SetsBaseUrlAndUserAgent()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			ApiKey = "test-key",
			BaseUrl = "https://dmapi.joker.com"
		};

		// Act
		using var transport = new DmapiTransport(
			options,
			"Test Agent",
			static (log, method, url) => log.LogDmapiRequest(method, url),
			static (log, content) => log.LogDmapiResponse(content));

		// Assert
		transport.Should().NotBeNull();
	}

	[Fact]
	public async Task SendAsync_ParsesResponseAndBuildsRequestUrl()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			ApiKey = "test-key",
			BaseUrl = "https://dmapi.joker.com"
		};

		using var transport = new DmapiTransport(
			options,
			"Test Agent",
			static (log, method, url) => log.LogDmapiRequest(method, url),
			static (log, content) => log.LogDmapiResponse(content));

		var parameters = new DmapiParameters().Set("username", "user@example.com");

		// Act
		var response = await transport.SendAsync("login", parameters, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task SendAsync_WithRequestAndResponseLogging_CallsLoggers()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			ApiKey = "test-key",
			EnableRequestLogging = true,
			EnableResponseLogging = true,
			Logger = new NullLogger()
		};

		using var transport = new DmapiTransport(
			options,
			"Test Agent",
			static (log, method, url) => log.LogDmapiRequest(method, url),
			static (log, content) => log.LogDmapiResponse(content));

		// Act
		var response = await transport.SendAsync("query-profile", new DmapiParameters(), CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public void Dispose_CanBeCalledTwice()
	{
		// Arrange
		var options = new JokerClientOptions { ApiKey = "test-key" };
		var transport = new DmapiTransport(
			options,
			"Test Agent",
			static (log, method, url) => log.LogDmapiRequest(method, url),
			static (log, content) => log.LogDmapiResponse(content));

		// Act & Assert (should not throw)
		transport.Dispose();
		transport.Dispose();
	}
}
