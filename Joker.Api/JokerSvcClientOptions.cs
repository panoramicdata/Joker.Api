namespace Joker.Api;

/// <summary>
/// Configuration options for the Joker SVC (Service/Dynamic DNS) client
/// </summary>
public class JokerSvcClientOptions
{
	/// <summary>
	/// Gets the domain name for SVC access
	/// </summary>
	public required string Domain { get; init; }

	/// <summary>
	/// Gets the SVC username (from Dynamic DNS settings in Joker.com dashboard)
	/// </summary>
	public required string SvcUsername { get; init; }

	/// <summary>
	/// Gets the SVC password (from Dynamic DNS settings in Joker.com dashboard)
	/// </summary>
	public required string SvcPassword { get; init; }

	/// <summary>
	/// Gets the DMAPI base URL (defaults to https://dmapi.joker.com)
	/// </summary>
	public string BaseUrl { get; init; } = "https://dmapi.joker.com";

	/// <summary>
	/// Gets the request timeout
	/// </summary>
	public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Gets the logger instance
	/// </summary>
	public Microsoft.Extensions.Logging.ILogger? Logger { get; init; }

	/// <summary>
	/// Gets a value indicating whether request logging is enabled
	/// </summary>
	public bool EnableRequestLogging { get; init; }

	/// <summary>
	/// Gets a value indicating whether response logging is enabled
	/// </summary>
	public bool EnableResponseLogging { get; init; }
}
