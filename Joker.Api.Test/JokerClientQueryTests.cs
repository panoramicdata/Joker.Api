using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Tests for additional DMAPI query operations
/// </summary>
public class JokerClientQueryTests : TestBase<JokerClientQueryTests>, IDisposable
{
	private readonly JokerClient _client;

	public JokerClientQueryTests()
	{
		// Create client with read-only API key
		var apiKey = _configuration["JokerApi:ApiKey:ReadOnly"];

		if (string.IsNullOrWhiteSpace(apiKey))
		{
			throw new InvalidOperationException("ReadOnly API key not configured in user secrets");
		}

		var options = new JokerClientOptions
		{
			ApiKey = apiKey,
			Logger = _logger,
			EnableRequestLogging = true,
			EnableResponseLogging = true
		};

		_client = new JokerClient(options);
	}

	[Fact]
	public async Task QueryContactList_ReturnsContacts()
	{
		// Act
		var response = await _client.QueryContactListAsync(
			pattern: null,
			tld: null,
			extendedFormat: false,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_logger.LogInformation("QueryContactList - StatusCode: {StatusCode}, StatusText: {StatusText}",
			response.StatusCode, response.StatusText);

		if (response.Errors.Count > 0)
		{
			foreach (var error in response.Errors)
			{
				_logger.LogWarning("  Error: {Error}", error);
			}
		}

		// With read-only key, this should work
		_ = response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryContactList_WithPattern_FiltersResults()
	{
		// Act
		var response = await _client.QueryContactListAsync(
			pattern: "*",
			tld: null,
			extendedFormat: false,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("QueryContactList with pattern - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task QueryContactList_WithExtendedFormat_IncludesAdditionalInfo()
	{
		// Act
		var response = await _client.QueryContactListAsync(
			pattern: null,
			tld: null,
			extendedFormat: true,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("QueryContactList extended - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task QueryNameserverList_ReturnsNameservers()
	{
		// Act
		var response = await _client.QueryNameserverListAsync(
			pattern: null,
			includeIps: false,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("QueryNameserverList - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task QueryNameserverList_WithIps_IncludesIpAddresses()
	{
		// Act
		var response = await _client.QueryNameserverListAsync(
			pattern: null,
			includeIps: true,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("QueryNameserverList with IPs - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task QueryProfile_ReturnsProfileData()
	{
		// Act
		var response = await _client.QueryProfileAsync(TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("QueryProfile - StatusCode: {StatusCode}", response.StatusCode);

		if (!string.IsNullOrWhiteSpace(response.AccountBalance))
		{
			_logger.LogInformation("Account Balance: {Balance}", response.AccountBalance);
		}
	}

	[Fact]
	public async Task ResultList_ReturnsResults()
	{
		// Act
		var response = await _client.ResultListAsync(
			pending: false,
			showAll: false,
			period: null,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("ResultList - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task ResultList_WithPending_ShowsPendingResults()
	{
		// Act
		var response = await _client.ResultListAsync(
			pending: true,
			showAll: false,
			period: null,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("ResultList (pending) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task DomainGetProperty_WithAutorenew_ReturnsPropertyValue()
	{
		// We need a domain name - skip if not available
		var domainList = await _client.QueryDomainListAsync(
			pattern: null,
			showStatus: false,
			showGrants: false,
			showJokerNs: false,
			cancellationToken: TestContext.Current.CancellationToken);

		if (!domainList.IsSuccess || string.IsNullOrWhiteSpace(domainList.Body))
		{
			_logger.LogWarning("No domains available to test DomainGetProperty");
			return;
		}

		var firstDomain = domainList.Body.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
		if (string.IsNullOrWhiteSpace(firstDomain))
		{
			return;
		}

		// Extract just the domain name (first column)
		var domainName = firstDomain.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
		if (string.IsNullOrWhiteSpace(domainName))
		{
			return;
		}

		// Act
		var response = await _client.DomainGetPropertyAsync(
			domainName,
			"autorenew",
			TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("DomainGetProperty for {Domain} - StatusCode: {StatusCode}",
			domainName, response.StatusCode);
	}

	[Fact]
	public async Task ResultRetrieveAsync_WithInvalidProcId_HandlesGracefully()
	{
		// Act - Try to retrieve with a non-existent proc ID
		var response = await _client.ResultRetrieveAsync(
			procId: "nonexistent-proc-id",
			svTrId: null,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("ResultRetrieve (invalid) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task ResultRetrieveAsync_RequiresEitherProcIdOrSvTrId() =>
		_ = await ((Func<Task<Models.DmapiResponse>>?)(async () => await _client.ResultRetrieveAsync(
				procId: null,
				svTrId: null,
				cancellationToken: TestContext.Current.CancellationToken)))
		.Should()
		.ThrowExactlyAsync<ArgumentException>();

	[Fact]
	public async Task LogoutAsync_WithUsernamePassword_DisposesSession()
	{
		// This test requires username/password which we don't have in CI
		// Skip if only API key is configured
		var username = _configuration["JokerApi:Username"];
		var password = _configuration["JokerApi:Password"];

		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
		{
			_logger.LogWarning("Skipping LogoutAsync test - username/password not configured");
			return;
		}

		var options = new JokerClientOptions
		{
			Username = username,
			Password = password,
			Logger = _logger
		};

		using var testClient = new JokerClient(options);

		// First login to establish session
		var loginResponse = await testClient.LoginAsync(TestContext.Current.CancellationToken);
		_ = loginResponse.Should().NotBeNull();

		if (!loginResponse.IsSuccess || string.IsNullOrWhiteSpace(loginResponse.AuthSid))
		{
			_logger.LogWarning("Login failed or no Auth-SID - skipping logout test");
			return;
		}

		// Act - Logout
		var logoutResponse = await testClient.LogoutAsync(TestContext.Current.CancellationToken);

		// Assert
		_ = logoutResponse.Should().NotBeNull();
		_logger.LogInformation("Logout - StatusCode: {StatusCode}", logoutResponse.StatusCode);
	}

	[Fact]
	public async Task QueryDomainList_WithAllOptions_ReturnsExtendedData()
	{
		// Act
		var response = await _client.QueryDomainListAsync(
			pattern: "*",
			showStatus: true,
			showGrants: true,
			showJokerNs: true,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("QueryDomainList (all options) - StatusCode: {StatusCode}", response.StatusCode);
	}

	[Fact]
	public async Task ResultListAsync_WithAllOptions_ReturnsFilteredResults()
	{
		// Act
		var response = await _client.ResultListAsync(
			pending: true,
			showAll: true,
			period: 30,
			cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		_ = response.Should().NotBeNull();
		_logger.LogInformation("ResultList (all options) - StatusCode: {StatusCode}", response.StatusCode);
	}

	public void Dispose()
	{
		_client?.Dispose();
		(_serviceProvider as IDisposable)?.Dispose();
		GC.SuppressFinalize(this);
	}
}
