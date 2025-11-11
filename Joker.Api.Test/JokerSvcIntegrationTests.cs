using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Joker.Api.Models;

namespace Joker.Api.Test;

/// <summary>
/// Integration tests for the Joker SVC (Dynamic DNS) client
/// These tests require SVC credentials from Dynamic DNS settings in Joker.com dashboard
/// </summary>
public class JokerSvcIntegrationTests : IDisposable
{
	private readonly JokerSvcClient? _client;
	private readonly ILogger<JokerSvcIntegrationTests> _logger;
	private readonly bool _hasCredentials;

	public JokerSvcIntegrationTests()
	{
		// Build configuration from user secrets
		var configuration = new ConfigurationBuilder()
			.AddUserSecrets<JokerSvcIntegrationTests>()
			.Build();

		// Setup logging
		var services = new ServiceCollection();
		services.AddLogging(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Debug);
		});

		var serviceProvider = services.BuildServiceProvider();
		_logger = serviceProvider.GetRequiredService<ILogger<JokerSvcIntegrationTests>>();

		// Check if SVC credentials are configured
		var domain = configuration["JokerSvc:Domain"];
		var svcUsername = configuration["JokerSvc:SvcUsername"];
		var svcPassword = configuration["JokerSvc:SvcPassword"];

		_hasCredentials = !string.IsNullOrWhiteSpace(domain) &&
		                  !string.IsNullOrWhiteSpace(svcUsername) &&
		                  !string.IsNullOrWhiteSpace(svcPassword);

		if (_hasCredentials)
		{
			var options = new JokerSvcClientOptions
			{
				Domain = domain!,
				SvcUsername = svcUsername!,
				SvcPassword = svcPassword!,
				Logger = _logger,
				EnableRequestLogging = true,
				EnableResponseLogging = true
			};

			_client = new JokerSvcClient(options);
		}
	}

	[Fact]
	public async Task GetDnsZone_WithSvcCredentials_RetrievesZone()
	{
		// Skip if no SVC credentials configured
		if (!_hasCredentials || _client == null)
		{
			_logger.LogWarning("Skipping test - no SVC credentials configured");
			_logger.LogInformation("To enable SVC tests, set JokerSvc:Domain, JokerSvc:SvcUsername, and JokerSvc:SvcPassword in user secrets");
			return;
		}

		// Act
		var response = await _client.GetDnsZoneAsync(TestContext.Current.CancellationToken);

		// Log response
		_logger.LogInformation("Response Status Code: {StatusCode}", response.StatusCode);
		_logger.LogInformation("Response Status Text: {StatusText}", response.StatusText);
		_logger.LogInformation("Response Result: {Result}", response.Result);
		
		if (response.Errors.Count > 0)
		{
			foreach (var error in response.Errors)
			{
				_logger.LogError("Error: {Error}", error);
			}
		}

		if (response.Body != null)
		{
			_logger.LogInformation("DNS Zone:\n{Zone}", response.Body);
		}

		// Assert
		Assert.NotNull(response);
		Assert.True(response.IsSuccess, 
			$"GetDnsZone failed: {response.StatusText}. Errors: {string.Join(", ", response.Errors)}");
		
		_logger.LogInformation("✓ SVC DNS zone retrieval successful");
	}

	[Fact]
	public async Task GetDnsZone_ParsesRecordTypes_CountsNonZero()
	{
		// Skip if no SVC credentials configured
		if (!_hasCredentials || _client == null)
		{
			_logger.LogWarning("Skipping test - no SVC credentials configured");
			return;
		}

		// Act
		var response = await _client.GetDnsZoneAsync(TestContext.Current.CancellationToken);

		// Assert response is successful
		Assert.True(response.IsSuccess, 
			$"GetDnsZone failed: {response.StatusText}");
		Assert.NotNull(response.Body);
		Assert.NotEmpty(response.Body);

		// Parse DNS records and count by type
		var lines = response.Body.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var recordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		foreach (var line in lines)
		{
			var trimmedLine = line.Trim();
			
			// Skip comments and empty lines
			if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(';'))
				continue;

			// Parse DNS record format: <label> <ttl> IN <type> <value>
			var parts = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length >= 4 && parts[2].Equals("IN", StringComparison.OrdinalIgnoreCase))
			{
				var recordType = parts[3].ToUpperInvariant();
				recordCounts[recordType] = recordCounts.GetValueOrDefault(recordType) + 1;
			}
		}

		// Log record counts
		_logger.LogInformation("DNS Record Type Counts:");
		foreach (var (recordType, count) in recordCounts.OrderBy(x => x.Key))
		{
			_logger.LogInformation("  {RecordType}: {Count}", recordType, count);
		}

		// Assert we have at least some common record types
		var totalRecords = recordCounts.Values.Sum();
		Assert.True(totalRecords > 0, "DNS zone should contain at least one record");
		
		_logger.LogInformation("✓ DNS zone contains {TotalRecords} total records", totalRecords);
		_logger.LogInformation("✓ Found {TypeCount} different record types", recordCounts.Count);

		// Most domains should have at least NS and SOA records
		var hasNsRecords = recordCounts.ContainsKey("NS");
		var hasSoaRecords = recordCounts.ContainsKey("SOA");
		
		if (hasNsRecords)
			_logger.LogInformation("✓ Zone has NS (nameserver) records");
		if (hasSoaRecords)
			_logger.LogInformation("✓ Zone has SOA (start of authority) records");
	}

	[Fact]
	public async Task SetTxtRecord_WithSvcCredentials_UpdatesRecord()
	{
		// Skip if no SVC credentials configured
		if (!_hasCredentials || _client == null)
		{
			_logger.LogWarning("Skipping test - no SVC credentials configured");
			return;
		}

		// Arrange
		var testLabel = "_joker-api-test";
		var testValue = $"test-{DateTime.UtcNow.Ticks}";

		// Act - Add TXT record
		_logger.LogInformation("Adding TXT record: {Label} = {Value}", testLabel, testValue);
		var addResponse = await _client.SetTxtRecordAsync(testLabel, testValue, 300, TestContext.Current.CancellationToken);

		// Log response
		_logger.LogInformation("Add Response Status: {Status}", addResponse.StatusText);
		
		if (addResponse.Errors.Count > 0)
		{
			foreach (var error in addResponse.Errors)
			{
				_logger.LogError("Error: {Error}", error);
			}
		}

		// Assert add succeeded
		Assert.NotNull(addResponse);
		Assert.True(addResponse.IsSuccess,
			$"SetTxtRecord failed: {addResponse.StatusText}. Errors: {string.Join(", ", addResponse.Errors)}");

		_logger.LogInformation("✓ TXT record added successfully");

		// Act - Delete TXT record (cleanup)
		_logger.LogInformation("Deleting TXT record: {Label}", testLabel);
		var deleteResponse = await _client.DeleteTxtRecordAsync(testLabel, TestContext.Current.CancellationToken);

		// Assert delete succeeded
		Assert.NotNull(deleteResponse);
		Assert.True(deleteResponse.IsSuccess,
			$"DeleteTxtRecord failed: {deleteResponse.StatusText}. Errors: {string.Join(", ", deleteResponse.Errors)}");

		_logger.LogInformation("✓ TXT record deleted successfully");
		_logger.LogInformation("✓ SVC DNS management test completed successfully");
	}

	public void Dispose()
	{
		_client?.Dispose();
		GC.SuppressFinalize(this);
	}
}
