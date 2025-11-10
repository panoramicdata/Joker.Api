namespace Joker.Api;

/// <summary>
/// Configuration options for the Joker API client
/// </summary>
public class JokerClientOptions
{
	/// <summary>
	/// Gets the Joker.com username (required if ApiKey is not provided)
	/// </summary>
	public string? Username { get; init; }

	/// <summary>
	/// Gets the Joker.com password (required if ApiKey is not provided)
	/// </summary>
	public string? Password { get; init; }

	/// <summary>
	/// Gets the DMAPI API key (alternative to Username/Password authentication)
	/// </summary>
	public string? ApiKey { get; init; }

	/// <summary>
	/// Gets the DMAPI base URL (defaults to https://dmapi.joker.com)
	/// </summary>
	public string BaseUrl { get; init; } = "https://dmapi.joker.com";

	/// <summary>
	/// Gets the request timeout
	/// </summary>
	public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Gets the maximum number of retry attempts
	/// </summary>
	public int MaxRetryAttempts { get; init; } = 3;

	/// <summary>
	/// Gets the delay between retry attempts
	/// </summary>
	public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(1);

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

	/// <summary>
	/// Gets a value indicating whether to use exponential backoff for retries
	/// </summary>
	public bool UseExponentialBackoff { get; init; } = true;

	/// <summary>
	/// Gets the maximum retry delay when using exponential backoff
	/// </summary>
	public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Validates the configuration options
	/// </summary>
	public void Validate()
	{
		if (string.IsNullOrWhiteSpace(ApiKey) && 
		    (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)))
		{
			throw new InvalidOperationException(
				"Either ApiKey or both Username and Password must be provided for authentication.");
		}
	}
}
