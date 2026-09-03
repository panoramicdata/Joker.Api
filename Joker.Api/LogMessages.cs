using Microsoft.Extensions.Logging;

namespace Joker.Api;

/// <summary>
/// High-performance logging methods using LoggerMessage source generation
/// </summary>
internal static class LogMessages
{
	private static readonly Action<ILogger, string, string, Exception?> DmapiRequest = LoggerMessage.Define<string, string>(
		LogLevel.Debug,
		new EventId(1, nameof(LogDmapiRequest)),
		"DMAPI Request: {Method} {Url}");

	private static readonly Action<ILogger, string, Exception?> DmapiResponse = LoggerMessage.Define<string>(
		LogLevel.Debug,
		new EventId(2, nameof(LogDmapiResponse)),
		"DMAPI Response: {Content}");

	private static readonly Action<ILogger, string, Exception?> SvcAuthenticated = LoggerMessage.Define<string>(
		LogLevel.Information,
		new EventId(3, nameof(LogSvcAuthenticated)),
		"SVC authenticated successfully for domain {Domain}");

	private static readonly Action<ILogger, string, Exception?> DnsZoneRetrieved = LoggerMessage.Define<string>(
		LogLevel.Information,
		new EventId(4, nameof(LogDnsZoneRetrieved)),
		"Retrieved DNS zone for {Domain}");

	private static readonly Action<ILogger, string, int, Exception?> DnsZoneUpdated = LoggerMessage.Define<string, int>(
		LogLevel.Information,
		new EventId(5, nameof(LogDnsZoneUpdated)),
		"Updated DNS zone for {Domain} with {Count} records");

	private static readonly Action<ILogger, string, string, Exception?> SvcDmapiRequest = LoggerMessage.Define<string, string>(
		LogLevel.Debug,
		new EventId(6, nameof(LogSvcDmapiRequest)),
		"SVC DMAPI Request: {Method} {Url}");

	private static readonly Action<ILogger, string, Exception?> SvcDmapiResponse = LoggerMessage.Define<string>(
		LogLevel.Debug,
		new EventId(7, nameof(LogSvcDmapiResponse)),
		"SVC DMAPI Response: {Content}");

	internal static void LogDmapiRequest(this ILogger logger, string method, string url) =>
		DmapiRequest(logger, method, url, null);

	internal static void LogDmapiResponse(this ILogger logger, string content) =>
		DmapiResponse(logger, content, null);

	internal static void LogSvcAuthenticated(this ILogger logger, string domain) =>
		SvcAuthenticated(logger, domain, null);

	internal static void LogDnsZoneRetrieved(this ILogger logger, string domain) =>
		DnsZoneRetrieved(logger, domain, null);

	internal static void LogDnsZoneUpdated(this ILogger logger, string domain, int count) =>
		DnsZoneUpdated(logger, domain, count, null);

	internal static void LogSvcDmapiRequest(this ILogger logger, string method, string url) =>
		SvcDmapiRequest(logger, method, url, null);

	internal static void LogSvcDmapiResponse(this ILogger logger, string content) =>
		SvcDmapiResponse(logger, content, null);
}
