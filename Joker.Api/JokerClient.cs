using Microsoft.Extensions.Logging;
using Joker.Api.Models;

namespace Joker.Api;

/// <summary>
/// Main client for interacting with the Joker DMAPI API
/// </summary>
public class JokerClient : JokerClientBase
{
	private readonly DmapiTransport _transport;
	private readonly JokerClientOptions _options;
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
		_transport = new DmapiTransport(
			options,
			"Joker.Api .NET Client",
			static (logger, method, url) => logger.LogDmapiRequest(method, url),
			static (logger, content) => logger.LogDmapiResponse(content));
	}

	/// <summary>
	/// Authenticates with the DMAPI and obtains a session ID
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The authentication response with session ID</returns>
	public async Task<DmapiResponse> LoginAsync(CancellationToken cancellationToken)
	{
		var parameters = new DmapiParameters();

		if (!string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			_ = parameters.Set("api-key", _options.ApiKey);
		}
		else
		{
			_ = parameters
				.Set("username", _options.Username!)
				.Set("password", _options.Password!);
		}

		var response = await _transport.SendAsync("login", parameters, cancellationToken).ConfigureAwait(false);

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
		var response = await _transport
			.SendAsync("logout", AddAuthParameters(new DmapiParameters()), cancellationToken)
			.ConfigureAwait(false);

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
		=> await SendAuthenticatedRequestAsync(
			"query-domain-list",
			new DmapiParameters()
				.SetIfPresent("pattern", pattern)
				.SetIfEnabled("showstatus", showStatus)
				.SetIfEnabled("showgrants", showGrants)
				.SetIfEnabled("showjokerns", showJokerNs),
			cancellationToken).ConfigureAwait(false);

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
		=> await SendAuthenticatedRequestAsync(
			"query-contact-list",
			new DmapiParameters()
				.SetIfPresent("pattern", pattern)
				.SetIfPresent("tld", tld)
				.SetIfEnabled("extended-format", extendedFormat),
			cancellationToken).ConfigureAwait(false);

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
		=> await SendAuthenticatedRequestAsync(
			"query-ns-list",
			new DmapiParameters()
				.SetIfPresent("pattern", pattern)
				.SetIfEnabled("full", includeIps),
			cancellationToken).ConfigureAwait(false);

	/// <summary>
	/// Queries reseller profile data (including account balance)
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The profile response</returns>
	public async Task<DmapiResponse> QueryProfileAsync(CancellationToken cancellationToken)
		=> await SendAuthenticatedRequestAsync(
			"query-profile",
			new DmapiParameters(),
			cancellationToken).ConfigureAwait(false);

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
		=> await SendAuthenticatedRequestAsync(
			"result-list",
			new DmapiParameters()
				.SetIfEnabled("pending", pending)
				.SetIfEnabled("showall", showAll)
				.SetIfPresent("period", period),
			cancellationToken).ConfigureAwait(false);

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

		return await SendAuthenticatedRequestAsync(
			"result-retrieve",
			new DmapiParameters()
				.SetIfPresent("proc-id", procId)
				.SetIfPresent("svtrid", svTrId),
			cancellationToken).ConfigureAwait(false);
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

		return await SendAuthenticatedRequestAsync(
			"domain-get-property",
			new DmapiParameters()
				.Set("domain", domain)
				.Set("pname", propertyName),
			cancellationToken).ConfigureAwait(false);
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

		return await SendAuthenticatedRequestAsync(
			"query-whois",
			new DmapiParameters().Set("domain", domain),
			cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Registers a new domain
	/// </summary>
	/// <param name="domain">Domain name to register</param>
	/// <param name="period">Registration period in years (1-10)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The registration response</returns>
	public async Task<DmapiResponse> DomainRegisterAsync(string domain, int period, CancellationToken cancellationToken)
		=> await SendDomainPeriodRequestAsync("domain-register", domain, period, cancellationToken).ConfigureAwait(false);

	/// <summary>
	/// Renews a domain
	/// </summary>
	/// <param name="domain">Domain name to renew</param>
	/// <param name="period">Renewal period in years (1-10)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The renewal response</returns>
	public async Task<DmapiResponse> DomainRenewAsync(string domain, int period, CancellationToken cancellationToken)
		=> await SendDomainPeriodRequestAsync("domain-renew", domain, period, cancellationToken).ConfigureAwait(false);

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

		return await SendAuthenticatedRequestAsync(
			"domain-transfer",
			new DmapiParameters()
				.Set("domain", domain)
				.Set("auth-code", authCode),
			cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Sends a request taking a validated domain and period, such as register and renew
	/// </summary>
	/// <param name="requestName">The request name (e.g., "domain-register")</param>
	/// <param name="domain">Domain name</param>
	/// <param name="period">Period in years (1-10)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The parsed DMAPI response</returns>
	private async Task<DmapiResponse> SendDomainPeriodRequestAsync(
		string requestName,
		string domain,
		int period,
		CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(domain);
		ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(period, 10);

		return await SendAuthenticatedRequestAsync(
			requestName,
			new DmapiParameters()
				.Set("domain", domain)
				.Set("period", period),
			cancellationToken).ConfigureAwait(false);
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
			_ = await LoginAsync(cancellationToken).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(_authSid))
			{
				throw new InvalidOperationException("Authentication failed. No auth-sid received.");
			}
		}
	}

	/// <summary>
	/// Adds the credential parameter (session ID, or API key when no session is held) to a request
	/// </summary>
	/// <param name="parameters">The parameters to add the credential to</param>
	/// <returns>The same parameters, for chaining</returns>
	private DmapiParameters AddAuthParameters(DmapiParameters parameters)
		=> string.IsNullOrWhiteSpace(_authSid)
			? parameters.SetIfPresent("api-key", _options.ApiKey)
			: parameters.Set("auth-sid", _authSid);

	/// <summary>
	/// Ensures the client is authenticated, adds the credential parameter and sends the request
	/// </summary>
	/// <param name="requestName">The request name (e.g., "query-domain-list")</param>
	/// <param name="parameters">Request parameters, which the credential is added to</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The parsed DMAPI response</returns>
	private async Task<DmapiResponse> SendAuthenticatedRequestAsync(
		string requestName,
		DmapiParameters parameters,
		CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);

		return await _transport
			.SendAsync(requestName, AddAuthParameters(parameters), cancellationToken)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Disposes the resources held by the client
	/// </summary>
	/// <param name="disposing">Whether to dispose managed resources</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_transport.Dispose();
		}

		base.Dispose(disposing);
	}
}
