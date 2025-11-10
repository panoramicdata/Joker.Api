using Microsoft.Extensions.Logging;
using Joker.Api.Models;

namespace Joker.Api;

/// <summary>
/// Main client for interacting with the Joker DMAPI API
/// </summary>
public class JokerClient : IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly JokerClientOptions _options;
	private bool _disposed;
	private string? _authSid;

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerClient"/> class
	/// </summary>
	/// <param name="options">Configuration options for the client</param>
	public JokerClient(JokerClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		options.Validate();

		_options = options;
		_httpClient = new HttpClient
		{
			BaseAddress = new Uri(options.BaseUrl),
			Timeout = options.RequestTimeout
		};

		_httpClient.DefaultRequestHeaders.Add("User-Agent", "Joker.Api .NET Client");
	}

	/// <summary>
	/// Authenticates with the DMAPI and obtains a session ID
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The authentication response with session ID</returns>
	public async Task<DmapiResponse> LoginAsync(CancellationToken cancellationToken = default)
	{
		var parameters = new Dictionary<string, string>();

		if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}
		else
		{
			parameters["username"] = _options.Username!;
			parameters["password"] = _options.Password!;
		}

		var response = await SendRequestAsync("login", parameters, cancellationToken).ConfigureAwait(false);

		if (response.IsSuccess && !string.IsNullOrWhiteSpace(response.AuthSid))
		{
			_authSid = response.AuthSid;
		}

		return response;
	}

	/// <summary>
	/// Sends a request to the DMAPI
	/// </summary>
	/// <param name="requestName">The request name (e.g., "login", "query-domain-list")</param>
	/// <param name="parameters">Request parameters</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The parsed DMAPI response</returns>
	private async Task<DmapiResponse> SendRequestAsync(
		string requestName,
		Dictionary<string, string>? parameters,
		CancellationToken cancellationToken)
	{
		// Build query string
		var queryParams = new List<string>();
		
		if (parameters != null)
		{
			foreach (var param in parameters)
			{
				queryParams.Add($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
			}
		}

		var url = $"/request/{requestName}";
		if (queryParams.Count > 0)
		{
			url += "?" + string.Join("&", queryParams);
		}

		if (_options.EnableRequestLogging)
		{
			_options.Logger?.LogDebug("DMAPI Request: {Method} {Url}", "GET", url);
		}

		var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
		var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		if (_options.EnableResponseLogging)
		{
			_options.Logger?.LogDebug("DMAPI Response: {Content}", content);
		}

		return ParseDmapiResponse(content);
	}

	/// <summary>
	/// Parses the DMAPI text-based response format
	/// </summary>
	/// <param name="content">The raw response content</param>
	/// <returns>Parsed DMAPI response</returns>
	private static DmapiResponse ParseDmapiResponse(string content)
	{
		var response = new DmapiResponse();
		var lines = content.Split('\n');
		var bodyStarted = false;
		var bodyLines = new List<string>();

		foreach (var line in lines)
		{
			var trimmedLine = line.TrimEnd('\r');

			// Empty line separates headers from body
			if (string.IsNullOrWhiteSpace(trimmedLine))
			{
				bodyStarted = true;
				continue;
			}

			if (!bodyStarted)
			{
				// Parse header line
				var colonIndex = trimmedLine.IndexOf(':');
				if (colonIndex > 0)
				{
					var headerName = trimmedLine[..colonIndex].Trim();
					var headerValue = trimmedLine[(colonIndex + 1)..].Trim();

					response.Headers[headerName] = headerValue;

					// Map known headers to properties
					switch (headerName.ToLowerInvariant())
					{
						case "auth-sid":
							response.AuthSid = headerValue;
							break;
						case "uid":
							response.Uid = headerValue;
							break;
						case "tracking-id":
							response.TrackingId = headerValue;
							break;
						case "status-code":
							_ = int.TryParse(headerValue, out var statusCode);
							response.StatusCode = statusCode;
							break;
						case "status-text":
							response.StatusText = headerValue;
							break;
						case "result":
							response.Result = headerValue;
							break;
						case "proc-id":
							response.ProcId = headerValue;
							break;
						case "account-balance":
							response.AccountBalance = headerValue;
							break;
						case "error":
							response.Errors.Add(headerValue);
							break;
						case "warning":
							response.Warnings.Add(headerValue);
							break;
					}
				}
			}
			else
			{
				// Collect body lines
				bodyLines.Add(trimmedLine);
			}
		}

		if (bodyLines.Count > 0)
		{
			response.Body = string.Join("\n", bodyLines);
		}

		return response;
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
