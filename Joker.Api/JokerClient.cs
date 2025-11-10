using Microsoft.Extensions.Logging;

namespace Joker.Api;

/// <summary>
/// Main client for interacting with the Joker DMAPI API
/// </summary>
public class JokerClient : IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly JokerClientOptions _options;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerClient"/> class
	/// </summary>
	/// <param name="options">Configuration options for the client</param>
	public JokerClient(JokerClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		_options = options;
		_httpClient = new HttpClient
		{
			BaseAddress = new Uri(options.BaseUrl),
			Timeout = options.RequestTimeout
		};

		_httpClient.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey);
		_httpClient.DefaultRequestHeaders.Add("User-Agent", "Joker.Api .NET Client");
	}

	/// <summary>
	/// Performs a GET request to the specified endpoint
	/// </summary>
	/// <typeparam name="T">The type of response expected</typeparam>
	/// <param name="endpoint">The API endpoint</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The deserialized response</returns>
	public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
	{
		if (_options.EnableRequestLogging)
		{
			_options.Logger?.LogDebug("GET {Endpoint}", endpoint);
		}

		var response = await _httpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		if (_options.EnableResponseLogging)
		{
			_options.Logger?.LogDebug("Response: {Content}", content);
		}

		return System.Text.Json.JsonSerializer.Deserialize<T>(content)
			?? throw new InvalidOperationException("Failed to deserialize response");
	}

	/// <summary>
	/// Performs a POST request to the specified endpoint
	/// </summary>
	/// <typeparam name="TRequest">The type of request body</typeparam>
	/// <typeparam name="TResponse">The type of response expected</typeparam>
	/// <param name="endpoint">The API endpoint</param>
	/// <param name="request">The request body</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The deserialized response</returns>
	public async Task<TResponse> PostAsync<TRequest, TResponse>(
		string endpoint,
		TRequest request,
		CancellationToken cancellationToken = default)
	{
		if (_options.EnableRequestLogging)
		{
			_options.Logger?.LogDebug("POST {Endpoint}", endpoint);
		}

		var json = System.Text.Json.JsonSerializer.Serialize(request);
		var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

		var response = await _httpClient.PostAsync(endpoint, content, cancellationToken).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		if (_options.EnableResponseLogging)
		{
			_options.Logger?.LogDebug("Response: {Content}", responseContent);
		}

		return System.Text.Json.JsonSerializer.Deserialize<TResponse>(responseContent)
			?? throw new InvalidOperationException("Failed to deserialize response");
	}

	/// <summary>
	/// Disposes the client and releases resources
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes the client and releases resources
	/// </summary>
	/// <param name="disposing">Whether to dispose managed resources</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_httpClient?.Dispose();
			}

			_disposed = true;
		}
	}
}
