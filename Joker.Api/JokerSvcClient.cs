using Microsoft.Extensions.Logging;
using Joker.Api.Models;

namespace Joker.Api;

/// <summary>
/// Client for Joker SVC (Service/Dynamic DNS) operations
/// Allows DNS management without a reseller account using Dynamic DNS credentials
/// </summary>
public class JokerSvcClient : IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly JokerSvcClientOptions _options;
	private readonly JokerClient _dmapiClient;
	private bool _disposed;
	private string? _authSid;

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerSvcClient"/> class
	/// </summary>
	/// <param name="options">Configuration options for the SVC client</param>
	public JokerSvcClient(JokerSvcClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		_options = options;
		_httpClient = new HttpClient
		{
			BaseAddress = new Uri(options.BaseUrl),
			Timeout = options.RequestTimeout
		};

		_httpClient.DefaultRequestHeaders.Add("User-Agent", "Joker.Api .NET SVC Client");

		// Create a DMAPI client for authentication
		_dmapiClient = new JokerClient(new JokerClientOptions
		{
			Username = options.SvcUsername,
			Password = options.SvcPassword,
			BaseUrl = options.BaseUrl,
			Logger = options.Logger,
			EnableRequestLogging = options.EnableRequestLogging,
			EnableResponseLogging = options.EnableResponseLogging
		});
	}

	/// <summary>
	/// Ensures we have a valid authentication session
	/// </summary>
	private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(_authSid))
		{
			var loginResponse = await _dmapiClient.LoginAsync(cancellationToken).ConfigureAwait(false);
			
			if (!loginResponse.IsSuccess)
			{
				throw new InvalidOperationException(
					$"SVC authentication failed: {loginResponse.StatusText}. " +
					$"Errors: {string.Join(", ", loginResponse.Errors)}");
			}

			_authSid = loginResponse.AuthSid;
			
			_options.Logger?.LogSvcAuthenticated(_options.Domain);
		}
	}

	/// <summary>
	/// Gets the current DNS zone configuration for the domain
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The DNS zone response</returns>
	public async Task<DmapiResponse> GetDnsZoneAsync(CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);

		var parameters = new Dictionary<string, string>
		{
			["auth-sid"] = _authSid!,
			["domain"] = _options.Domain
		};

		var response = await SendDmapiRequestAsync("dns-zone-get", parameters, cancellationToken).ConfigureAwait(false);

		if (response.IsSuccess)
		{
			_options.Logger?.LogDnsZoneRetrieved(_options.Domain);
		}

		return response;
	}

	/// <summary>
	/// Updates the DNS zone with the specified records
	/// </summary>
	/// <param name="records">The DNS records to set</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The response from the DNS zone update</returns>
	public async Task<DmapiResponse> SetDnsZoneAsync(IEnumerable<DnsRecord> records, CancellationToken cancellationToken)
	{
		await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);

		var recordList = records as ICollection<DnsRecord> ?? [.. records];
		var zoneData = string.Join("\n", recordList.Select(r => r.ToZoneFormat()));

		var parameters = new Dictionary<string, string>
		{
			["auth-sid"] = _authSid!,
			["domain"] = _options.Domain,
			["zone"] = zoneData
		};

		var response = await SendDmapiRequestAsync("dns-zone-put", parameters, cancellationToken).ConfigureAwait(false);

		if (response.IsSuccess)
		{
			_options.Logger?.LogDnsZoneUpdated(_options.Domain, recordList.Count);
		}

		return response;
	}

	/// <summary>
	/// Adds or updates a TXT record (useful for ACME DNS-01 challenges)
	/// </summary>
	/// <param name="label">The subdomain label (e.g., "_acme-challenge")</param>
	/// <param name="value">The TXT record value</param>
	/// <param name="ttl">Optional TTL in seconds</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The response from the DNS update</returns>
	public async Task<DmapiResponse> SetTxtRecordAsync(
		string label, 
		string value, 
		int? ttl,
		CancellationToken cancellationToken)
		=> await ReplaceTxtRecordAsync(label, DnsRecord.CreateTxtRecord(label, value, ttl), cancellationToken).ConfigureAwait(false);

	/// <summary>
	/// Deletes a TXT record
	/// </summary>
	/// <param name="label">The subdomain label to delete</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The response from the DNS update</returns>
	public async Task<DmapiResponse> DeleteTxtRecordAsync(
		string label,
		CancellationToken cancellationToken)
		=> await ReplaceTxtRecordAsync(label, null, cancellationToken).ConfigureAwait(false);

	/// <summary>
	/// Rewrites the zone with every TXT record for <paramref name="label"/> removed, optionally
	/// putting <paramref name="replacement"/> in their place
	/// </summary>
	/// <param name="label">The subdomain label whose TXT records are replaced</param>
	/// <param name="replacement">The record to add, or null to only remove</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The response from the DNS update</returns>
	private async Task<DmapiResponse> ReplaceTxtRecordAsync(
		string label,
		DnsRecord? replacement,
		CancellationToken cancellationToken)
	{
		var currentZone = await GetDnsZoneAsync(cancellationToken).ConfigureAwait(false);

		if (!currentZone.IsSuccess)
		{
			throw new InvalidOperationException(
				$"Failed to retrieve current DNS zone: {currentZone.StatusText}");
		}

		// Keep every record except the TXT records carrying this label
		var updatedRecords = ParseZoneRecords(currentZone.Body ?? string.Empty)
			.Where(r => !(r.Type.Equals("TXT", StringComparison.OrdinalIgnoreCase) &&
			             r.Label.Equals(label, StringComparison.OrdinalIgnoreCase)))
			.ToList();

		if (replacement != null)
		{
			updatedRecords.Add(replacement);
		}

		return await SetDnsZoneAsync(updatedRecords, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Parses zone data into DNS records
	/// </summary>
	private static List<DnsRecord> ParseZoneRecords(string zoneData)
	{
		var records = new List<DnsRecord>();
		
		if (string.IsNullOrWhiteSpace(zoneData))
		{
			return records;
		}

		foreach (var line in zoneData.Split('\n'))
		{
			var record = ParseZoneRecordLine(line);
			if (record != null)
			{
				records.Add(record);
			}
		}

		return records;
	}

	/// <summary>
	/// Parses a single zone record line
	/// </summary>
	private static DnsRecord? ParseZoneRecordLine(string line)
	{
		var trimmedLine = line.Trim();
		if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
		{
			return null;
		}

		var parts = trimmedLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length < 3)
		{
			return null;
		}

		var record = new DnsRecord
		{
			Type = parts[0],
			Label = parts[1],
			Value = parts[^1] // Last part is always the value
		};

		// Parse optional fields based on record type
		if (parts.Length > 3)
		{
			ParseOptionalFields(parts, record);
		}

		return record;
	}

	/// <summary>
	/// Parses optional fields (priority, TTL) from record parts
	/// </summary>
	private static void ParseOptionalFields(string[] parts, DnsRecord record)
	{
		// For MX records: Type:Label:Priority:Value
		if (record.Type.Equals("MX", StringComparison.OrdinalIgnoreCase) && int.TryParse(parts[2], out var priority))
		{
			record.Priority = priority;
			if (parts.Length > 4 && int.TryParse(parts[4], out var ttl))
			{
				record.Ttl = ttl;
			}
		}
		// For other records with TTL: Type:Label:Value:TTL
		else if (int.TryParse(parts[^1], out var ttl))
		{
			record.Ttl = ttl;
			record.Value = parts[^2];
		}
	}

	/// <summary>
	/// Sends a request to the DMAPI
	/// </summary>
	private async Task<DmapiResponse> SendDmapiRequestAsync(
		string requestName,
		Dictionary<string, string> parameters,
		CancellationToken cancellationToken)
	{
		var url = DmapiRequestUrlBuilder.Build(requestName, parameters);

		if (_options.EnableRequestLogging)
		{
			_options.Logger?.LogSvcDmapiRequest("GET", url);
		}

		var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
		var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		if (_options.EnableResponseLogging)
		{
			_options.Logger?.LogSvcDmapiResponse(content);
		}

		return DmapiResponseParser.Parse(content);
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
				_dmapiClient?.Dispose();
			}

			_disposed = true;
		}
	}
}
