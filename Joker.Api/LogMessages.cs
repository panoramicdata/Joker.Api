using Microsoft.Extensions.Logging;

namespace Joker.Api;

/// <summary>
/// High-performance logging methods using LoggerMessage source generation
/// </summary>
internal static partial class LogMessages
{
	[LoggerMessage(Level = LogLevel.Debug, Message = "DMAPI Request: {Method} {Url}")]
	internal static partial void LogDmapiRequest(this ILogger logger, string method, string url);

	[LoggerMessage(Level = LogLevel.Debug, Message = "DMAPI Response: {Content}")]
	internal static partial void LogDmapiResponse(this ILogger logger, string content);

	[LoggerMessage(Level = LogLevel.Information, Message = "SVC authenticated successfully for domain {Domain}")]
	internal static partial void LogSvcAuthenticated(this ILogger logger, string domain);

	[LoggerMessage(Level = LogLevel.Information, Message = "Retrieved DNS zone for {Domain}")]
	internal static partial void LogDnsZoneRetrieved(this ILogger logger, string domain);

	[LoggerMessage(Level = LogLevel.Information, Message = "Updated DNS zone for {Domain} with {Count} records")]
	internal static partial void LogDnsZoneUpdated(this ILogger logger, string domain, int count);

	[LoggerMessage(Level = LogLevel.Debug, Message = "SVC DMAPI Request: {Method} {Url}")]
	internal static partial void LogSvcDmapiRequest(this ILogger logger, string method, string url);

	[LoggerMessage(Level = LogLevel.Debug, Message = "SVC DMAPI Response: {Content}")]
	internal static partial void LogSvcDmapiResponse(this ILogger logger, string content);
}
