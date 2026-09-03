using Microsoft.Extensions.Logging;
using Joker.Api.Models;

namespace Joker.Api;

/// <summary>
/// Owns the HTTP connection to a DMAPI endpoint and exchanges requests for parsed responses.
/// </summary>
internal sealed class DmapiTransport : IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly IDmapiTransportOptions _options;
	private readonly Action<ILogger, string, string> _logRequest;
	private readonly Action<ILogger, string> _logResponse;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="DmapiTransport"/> class
	/// </summary>
	/// <param name="options">The endpoint and logging options</param>
	/// <param name="userAgent">The User-Agent header identifying the calling client</param>
	/// <param name="logRequest">Writes the method and URL of a request to a logger</param>
	/// <param name="logResponse">Writes the raw content of a response to a logger</param>
	internal DmapiTransport(
		IDmapiTransportOptions options,
		string userAgent,
		Action<ILogger, string, string> logRequest,
		Action<ILogger, string> logResponse)
	{
		_options = options;
		_logRequest = logRequest;
		_logResponse = logResponse;

		_httpClient = new HttpClient
		{
			BaseAddress = new Uri(options.BaseUrl),
			Timeout = options.RequestTimeout
		};

		_httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
	}

	/// <summary>
	/// Sends a DMAPI request and parses the response
	/// </summary>
	/// <param name="requestName">The request name (e.g., "login", "query-domain-list")</param>
	/// <param name="parameters">Request parameters</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The parsed DMAPI response</returns>
	internal async Task<DmapiResponse> SendAsync(
		string requestName,
		DmapiParameters parameters,
		CancellationToken cancellationToken)
	{
		var url = DmapiRequestUrlBuilder.Build(requestName, parameters.Values);

		if (_options.EnableRequestLogging && _options.Logger is not null)
		{
			_logRequest(_options.Logger, "GET", url);
		}

		var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
		var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		if (_options.EnableResponseLogging && _options.Logger is not null)
		{
			_logResponse(_options.Logger, content);
		}

		return DmapiResponseParser.Parse(content);
	}

	/// <summary>
	/// Disposes the underlying <see cref="HttpClient"/>
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_httpClient.Dispose();
		_disposed = true;
	}
}
