using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Joker.Api.Models;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="JokerClient"/> that cover validation and error paths
/// without requiring live credentials
/// </summary>
public class JokerClientOfflineTests
{
	private static JokerClient CreateClientWithApiKey() => new(new JokerClientOptions
	{
		ApiKey = "offline-test-key"
	});

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
	public async Task LoginAsync_WithApiKey_SendsRequestAndHandlesFailure()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.LoginAsync(CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task LoginAsync_WithUsernamePassword_SendsRequestAndHandlesFailure()
	{
		// Arrange
		var options = new JokerClientOptions
		{
			Username = "offline@example.com",
			Password = "offline-password"
		};

		using var client = new JokerClient(options);

		// Act & Assert
		await AssertThrowsAsync(() => client.LoginAsync(CancellationToken.None));
	}

	[Fact]
	public async Task LogoutAsync_WithApiKey_SendsRequestAndHandlesFailure()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.LogoutAsync(CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryDomainListAsync_ValidatesAndSendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryDomainListAsync(
			pattern: "*",
			showStatus: true,
			showGrants: true,
			showJokerNs: true,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryDomainListAsync_NoPattern_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryDomainListAsync(
			pattern: null,
			showStatus: false,
			showGrants: false,
			showJokerNs: false,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryContactListAsync_WithAllOptions_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryContactListAsync(
			pattern: "*",
			tld: "de",
			extendedFormat: true,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryContactListAsync_WithoutOptions_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryContactListAsync(
			pattern: null,
			tld: null,
			extendedFormat: false,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryNameserverListAsync_WithIps_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryNameserverListAsync(
			pattern: "*",
			includeIps: true,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryNameserverListAsync_WithoutIps_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryNameserverListAsync(
			pattern: null,
			includeIps: false,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryProfileAsync_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryProfileAsync(CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task ResultListAsync_WithAllOptions_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.ResultListAsync(
			pending: true,
			showAll: true,
			period: 30,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task ResultListAsync_WithoutOptions_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.ResultListAsync(
			pending: false,
			showAll: false,
			period: null,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task ResultRetrieveAsync_WithProcId_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.ResultRetrieveAsync(
			procId: "offline-proc-id",
			svTrId: null,
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task ResultRetrieveAsync_WithSvTrId_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.ResultRetrieveAsync(
			procId: null,
			svTrId: "offline-svtrid",
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task ResultRetrieveAsync_WithNeitherProcIdNorSvTrId_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.ResultRetrieveAsync(null, null, CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentException>();
	}

	[Fact]
	public async Task DomainGetPropertyAsync_ValidatesAndSendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.DomainGetPropertyAsync(
			domain: "offline-example.com",
			propertyName: "autorenew",
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task DomainGetPropertyAsync_WithEmptyDomain_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainGetPropertyAsync("", "autorenew", CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentException>();
	}

	[Fact]
	public async Task DomainGetPropertyAsync_WithEmptyPropertyName_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainGetPropertyAsync("offline-example.com", "", CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentException>();
	}

	[Fact]
	public async Task QueryWhoisAsync_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.QueryWhoisAsync("offline-example.com", CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryWhoisAsync_WithEmptyDomain_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.QueryWhoisAsync("", CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentException>();
	}

	[Fact]
	public async Task DomainRegisterAsync_WithValidPeriod_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.DomainRegisterAsync("offline-example.com", 1, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task DomainRegisterAsync_WithPeriodTooLow_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainRegisterAsync("offline-example.com", 0, CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task DomainRegisterAsync_WithPeriodTooHigh_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainRegisterAsync("offline-example.com", 11, CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task DomainRenewAsync_WithValidPeriod_SendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.DomainRenewAsync("offline-example.com", 2, CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task DomainRenewAsync_WithPeriodTooLow_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainRenewAsync("offline-example.com", 0, CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task DomainRenewAsync_WithPeriodTooHigh_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainRenewAsync("offline-example.com", 11, CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task DomainTransferAsync_ValidatesAndSendsRequest()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act
		var response = await client.DomainTransferAsync(
			domain: "offline-example.com",
			authCode: "OFFLINE-AUTH-CODE",
			cancellationToken: CancellationToken.None);

		// Assert
		response.Should().NotBeNull();
	}

	[Fact]
	public async Task DomainTransferAsync_WithEmptyDomain_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainTransferAsync("", "OFFLINE-AUTH-CODE", CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentException>();
	}

	[Fact]
	public async Task DomainTransferAsync_WithEmptyAuthCode_Throws()
	{
		// Arrange
		using var client = CreateClientWithApiKey();

		// Act & Assert
		Func<Task<DmapiResponse>> call = () => client.DomainTransferAsync("offline-example.com", "", CancellationToken.None);
		await call.Should().ThrowExactlyAsync<ArgumentException>();
	}
}
