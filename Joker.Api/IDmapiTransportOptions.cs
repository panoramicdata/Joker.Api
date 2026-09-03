using Microsoft.Extensions.Logging;

namespace Joker.Api;

/// <summary>
/// The options a <see cref="DmapiTransport"/> needs, as supplied by each client's options class.
/// </summary>
internal interface IDmapiTransportOptions
{
	/// <summary>
	/// The base URL of the DMAPI endpoint
	/// </summary>
	string BaseUrl { get; }

	/// <summary>
	/// The per-request timeout
	/// </summary>
	TimeSpan RequestTimeout { get; }

	/// <summary>
	/// The logger to write request and response diagnostics to, if any
	/// </summary>
	ILogger? Logger { get; }

	/// <summary>
	/// Whether outgoing requests are logged
	/// </summary>
	bool EnableRequestLogging { get; }

	/// <summary>
	/// Whether incoming responses are logged
	/// </summary>
	bool EnableResponseLogging { get; }
}
