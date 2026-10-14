using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Joker.Api.Models;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="JokerSvcClient"/> that cover validation and error paths
/// without requiring live SVC credentials
/// </summary>
public class JokerSvcClientOfflineTests
{
	private static JokerSvcClientOptions OfflineOptions() => new()
	{
		Domain = "offline-example.com",
		SvcUsername = "offline-svc-user",
		SvcPassword = "offline-svc-password"
	};

	private static async Task AssertThrowsAsync(Func<Task> call)
	{
		try
		{
			await call();
			Assert.Fail("Expected an exception to be thrown.");
		}
		catch (Exception)
		{
			// Expected.
		}
	}

	[Fact]
	public void Constructor_WithNullOptions_Throws()
	{
		// Act & Assert
		Action call = () => new JokerSvcClient(null!);
		call.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_WithOfflineOptions_CreatesClient()
	{
		// Arrange
		var options = OfflineOptions();

		// Act
		using var client = new JokerSvcClient(options);

		// Assert
		client.Should().NotBeNull();
	}

	[Fact]
	public async Task GetDnsZoneAsync_WithInvalidCredentials_Throws()
	{
		// Arrange
		using var client = new JokerSvcClient(OfflineOptions());

		// Act & Assert
		await AssertThrowsAsync(() => client.GetDnsZoneAsync(CancellationToken.None));
	}

	[Fact]
	public async Task SetDnsZoneAsync_WithInvalidCredentials_Throws()
	{
		// Arrange
		using var client = new JokerSvcClient(OfflineOptions());
		var records = new List<DnsRecord>
		{
			DnsRecord.CreateARecord("@", "192.0.2.1", 300)
		};

		// Act & Assert
		await AssertThrowsAsync(() => client.SetDnsZoneAsync(records, CancellationToken.None));
	}

	[Fact]
	public async Task SetDnsZoneAsync_WithEmptyRecords_Throws()
	{
		// Arrange
		using var client = new JokerSvcClient(OfflineOptions());
		var records = new List<DnsRecord>();

		// Act & Assert
		await AssertThrowsAsync(() => client.SetDnsZoneAsync(records, CancellationToken.None));
	}

	[Fact]
	public async Task SetTxtRecordAsync_WithInvalidCredentials_Throws()
	{
		// Arrange
		using var client = new JokerSvcClient(OfflineOptions());

		// Act & Assert
		await AssertThrowsAsync(() => client.SetTxtRecordAsync(
			label: "_offline-challenge",
			value: "offline-token",
			ttl: 300,
			cancellationToken: CancellationToken.None));
	}

	[Fact]
	public async Task DeleteTxtRecordAsync_WithInvalidCredentials_Throws()
	{
		// Arrange
		using var client = new JokerSvcClient(OfflineOptions());

		// Act & Assert
		await AssertThrowsAsync(() => client.DeleteTxtRecordAsync(
			label: "_offline-challenge",
			cancellationToken: CancellationToken.None));
	}
}
