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
	public async Task<DmapiResponse> LoginAsync(CancellationToken cancellationToken)
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
	/// Logs out and invalidates the session
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The logout response</returns>
	public async Task<DmapiResponse> LogoutAsync(CancellationToken cancellationToken)
	{
		var parameters = new Dictionary<string, string>();
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		var response = await SendRequestAsync("logout", parameters, cancellationToken).ConfigureAwait(false);

		if (response.IsSuccess)
		{
			_authSid = null;
		}

		return response;
	}

	/// <summary>
	/// Queries the list of domains in the account
	/// </summary>
	/// <param name="pattern">Optional pattern to match (glob-like)</param>
	/// <param name="showStatus">Add domain status column</param>
	/// <param name="showGrants">Add domain grants column</param>
	/// <param name="showJokerNs">Add column showing if domain uses Joker nameservers</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The domain list response</returns>
	public async Task<DmapiResponse> QueryDomainListAsync(
		string? pattern,
		bool showStatus,
		bool showGrants,
		bool showJokerNs,
		CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>();
		
		if (!string.IsNullOrWhiteSpace(pattern))
		{
			parameters["pattern"] = pattern;
		}
		
		if (showStatus)
		{
			parameters["showstatus"] = "1";
		}
		
		if (showGrants)
		{
			parameters["showgrants"] = "1";
		}
		
		if (showJokerNs)
		{
			parameters["showjokerns"] = "1";
		}
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("query-domain-list", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Queries the list of contact handles
	/// </summary>
	/// <param name="pattern">Optional pattern to match against handle</param>
	/// <param name="tld">Optional TLD to limit output to contacts usable with specified TLD</param>
	/// <param name="extendedFormat">Include additional information (name and organization)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The contact list response</returns>
	public async Task<DmapiResponse> QueryContactListAsync(
		string? pattern,
		string? tld,
		bool extendedFormat,
		CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>();
		
		if (!string.IsNullOrWhiteSpace(pattern))
		{
			parameters["pattern"] = pattern;
		}
		
		if (!string.IsNullOrWhiteSpace(tld))
		{
			parameters["tld"] = tld;
		}
		
		if (extendedFormat)
		{
			parameters["extended-format"] = "1";
		}
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("query-contact-list", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Queries the list of nameserver/host handles
	/// </summary>
	/// <param name="pattern">Optional pattern to match against host name</param>
	/// <param name="includeIps">Include IP addresses (IPv4 and IPv6)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The nameserver list response</returns>
	public async Task<DmapiResponse> QueryNameserverListAsync(
		string? pattern,
		bool includeIps,
		CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>();
		
		if (!string.IsNullOrWhiteSpace(pattern))
		{
			parameters["pattern"] = pattern;
		}
		
		if (includeIps)
		{
			parameters["full"] = "1";
		}
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("query-ns-list", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Queries reseller profile data (including account balance)
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The profile response</returns>
	public async Task<DmapiResponse> QueryProfileAsync(CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>();
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("query-profile", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Lists results from asynchronous requests
	/// </summary>
	/// <param name="pending">Show results without reply (in progress)</param>
	/// <param name="showAll">Show results deleted using result-delete</param>
	/// <param name="period">Show results for specified period of days</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result list response</returns>
	public async Task<DmapiResponse> ResultListAsync(
		bool pending,
		bool showAll,
		int? period,
		CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>();
		
		if (pending)
		{
			parameters["pending"] = "1";
		}
		
		if (showAll)
		{
			parameters["showall"] = "1";
		}
		
		if (period.HasValue)
		{
			parameters["period"] = period.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("result-list", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Retrieves result from an asynchronous request
	/// </summary>
	/// <param name="procId">Processing ID (optional if svTrId provided)</param>
	/// <param name="svTrId">Server tracking ID (optional if procId provided)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result response</returns>
	public async Task<DmapiResponse> ResultRetrieveAsync(
		string? procId,
		string? svTrId,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(procId) && string.IsNullOrWhiteSpace(svTrId))
		{
			throw new ArgumentException("Either procId or svTrId must be provided");
		}
		
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>();
		
		if (!string.IsNullOrWhiteSpace(procId))
		{
			parameters["proc-id"] = procId;
		}
		
		if (!string.IsNullOrWhiteSpace(svTrId))
		{
			parameters["svtrid"] = svTrId;
		}
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("result-retrieve", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets a domain property value
	/// </summary>
	/// <param name="domain">Domain name</param>
	/// <param name="propertyName">Property name (e.g., "autorenew", "whois-opt-out", "privacy")</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The property value response</returns>
	public async Task<DmapiResponse> DomainGetPropertyAsync(
		string domain,
		string propertyName,
		CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(domain);
		ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>
		{
			["domain"] = domain,
			["pname"] = propertyName
		};
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("domain-get-property", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Queries WHOIS information for a domain
	/// </summary>
	/// <param name="domain">Domain name to query</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The WHOIS response</returns>
	public async Task<DmapiResponse> QueryWhoisAsync(string domain, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(domain);
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>
		{
			["domain"] = domain
		};
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("query-whois", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Registers a new domain
	/// </summary>
	/// <param name="domain">Domain name to register</param>
	/// <param name="period">Registration period in years (1-10)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The registration response</returns>
	public async Task<DmapiResponse> DomainRegisterAsync(string domain, int period, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(domain);
		ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(period, 10);
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>
		{
			["domain"] = domain,
			["period"] = period.ToString(System.Globalization.CultureInfo.InvariantCulture)
		};
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("domain-register", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Renews a domain
	/// </summary>
	/// <param name="domain">Domain name to renew</param>
	/// <param name="period">Renewal period in years (1-10)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The renewal response</returns>
	public async Task<DmapiResponse> DomainRenewAsync(string domain, int period, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(domain);
		ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(period, 10);
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>
		{
			["domain"] = domain,
			["period"] = period.ToString(System.Globalization.CultureInfo.InvariantCulture)
		};
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("domain-renew", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Transfers a domain to this account
	/// </summary>
	/// <param name="domain">Domain name to transfer</param>
	/// <param name="authCode">Authorization code for transfer</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The transfer response</returns>
	public async Task<DmapiResponse> DomainTransferAsync(string domain, string authCode, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(domain);
		ArgumentException.ThrowIfNullOrWhiteSpace(authCode);
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
		
		var parameters = new Dictionary<string, string>
		{
			["domain"] = domain,
			["auth-code"] = authCode
		};
		
		if (!string.IsNullOrWhiteSpace(_authSid))
		{
			parameters["auth-sid"] = _authSid;
		}
		else if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			parameters["api-key"] = _options.ApiKey;
		}

		return await SendRequestAsync("domain-transfer", parameters, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Ensures the client is authenticated (calls LoginAsync if using username/password and not yet authenticated)
	/// </summary>
	private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
	{
		// If using API key, we don't need to call login
		if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			return;
		}
		
		// If not authenticated yet and using username/password, login now
		if (string.IsNullOrWhiteSpace(_authSid))
		{
			await LoginAsync(cancellationToken).ConfigureAwait(false);
			
			if (string.IsNullOrWhiteSpace(_authSid))
			{
				throw new InvalidOperationException("Authentication failed. No auth-sid received.");
			}
		}
	}

	/// <summary>
	/// Ensures the client is authenticated (for sync methods - throws if not)
	/// </summary>
	private void EnsureAuthenticated()
	{
		if (string.IsNullOrWhiteSpace(_authSid) && string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			throw new InvalidOperationException("Not authenticated. Call LoginAsync first or use API key authentication.");
		}
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
				ParseHeaderLine(trimmedLine, response);
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
	/// Parses a single header line and updates the response object
	/// </summary>
	/// <param name="line">The header line to parse</param>
	/// <param name="response">The response object to update</param>
	private static void ParseHeaderLine(string line, DmapiResponse response)
	{
		var colonIndex = line.IndexOf(':');
		if (colonIndex <= 0)
		{
			return;
		}

		var headerName = line[..colonIndex].Trim();
		var headerValue = line[(colonIndex + 1)..].Trim();

		response.Headers[headerName] = headerValue;

		// Map known headers to properties
		MapHeaderToProperty(headerName, headerValue, response);
	}

	/// <summary>
	/// Maps known header names to response properties
	/// </summary>
	/// <param name="headerName">The header name</param>
	/// <param name="headerValue">The header value</param>
	/// <param name="response">The response object to update</param>
	private static void MapHeaderToProperty(string headerName, string headerValue, DmapiResponse response)
	{
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
			default:
				// Unknown header - already stored in Headers dictionary
				break;
		}
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
