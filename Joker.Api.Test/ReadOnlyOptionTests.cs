using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Joker.Api.Models;

namespace Joker.Api.Test;

/// <summary>
/// Credential-free tests of <c>IsReadOnly</c> on both clients, against a loopback stub DMAPI endpoint
/// </summary>
public sealed class ReadOnlyOptionTests : IDisposable
{
	private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
	private readonly List<string> _requests = [];
	private readonly CancellationTokenSource _stop = new();

	public ReadOnlyOptionTests()
	{
		_listener.Start();
		_ = Task.Run(ServeAsync);
	}

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private string BaseUrl => $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}";

	private JokerClient CreateClient(bool isReadOnly) => new(new JokerClientOptions
	{
		ApiKey = "test-key",
		BaseUrl = BaseUrl,
		IsReadOnly = isReadOnly
	});

	private JokerSvcClient CreateSvcClient(bool isReadOnly) => new(new JokerSvcClientOptions
	{
		Domain = "example.com",
		SvcUsername = "user",
		SvcPassword = "password",
		BaseUrl = BaseUrl,
		IsReadOnly = isReadOnly
	});

	[Fact]
	public void IsReadOnly_DefaultsToFalse()
	{
		new JokerClientOptions { ApiKey = "k" }.IsReadOnly.Should().BeFalse();
		new JokerSvcClientOptions { Domain = "d", SvcUsername = "u", SvcPassword = "p" }.IsReadOnly.Should().BeFalse();
	}

	[Theory]
	[InlineData("domain-register")]
	[InlineData("domain-renew")]
	[InlineData("domain-transfer")]
	public async Task JokerClientWrite_WhenReadOnly_ThrowsAndSendsNothing(string request)
	{
		using var client = CreateClient(isReadOnly: true);

		var act = () => request switch
		{
			"domain-register" => client.DomainRegisterAsync("example.com", 1, CancellationToken),
			"domain-renew" => client.DomainRenewAsync("example.com", 1, CancellationToken),
			_ => client.DomainTransferAsync("example.com", "auth", CancellationToken)
		};

		await act.Should().ThrowAsync<InvalidOperationException>();
		_requests.Should().BeEmpty();
	}

	[Fact]
	public async Task JokerClientWrite_WhenNotReadOnly_IsSent()
	{
		using var client = CreateClient(isReadOnly: false);

		_ = await client.DomainRenewAsync("example.com", 1, CancellationToken);

		_requests.Should().ContainSingle().Which.Should().StartWith("GET /request/domain-renew");
	}

	[Fact]
	public async Task JokerClientQuery_WhenReadOnly_IsStillSent()
	{
		using var client = CreateClient(isReadOnly: true);

		_ = await client.QueryProfileAsync(CancellationToken);

		_requests.Should().ContainSingle().Which.Should().StartWith("GET /request/query-profile");
	}

	[Fact]
	public async Task SvcClientWrite_WhenReadOnly_ThrowsBeforeLoginOrSend()
	{
		using var client = CreateSvcClient(isReadOnly: true);

		var act = () => client.SetDnsZoneAsync([], CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
		_requests.Should().BeEmpty();
	}

	[Fact]
	public async Task SvcClientRead_WhenReadOnly_IsStillSent()
	{
		using var client = CreateSvcClient(isReadOnly: true);

		_ = await client.GetDnsZoneAsync(CancellationToken);

		_requests.Should().HaveCount(2);
		_requests[0].Should().StartWith("GET /request/login");
		_requests[1].Should().StartWith("GET /request/dns-zone-get");
	}

	[Theory]
	[InlineData("login", true)]
	[InlineData("logout", true)]
	[InlineData("query-domain-list", true)]
	[InlineData("query-whois", true)]
	[InlineData("result-list", true)]
	[InlineData("result-retrieve", true)]
	[InlineData("domain-get-property", true)]
	[InlineData("dns-zone-get", true)]
	[InlineData("dns-zone-put", false)]
	[InlineData("domain-register", false)]
	[InlineData("domain-renew", false)]
	[InlineData("domain-transfer", false)]
	[InlineData("domain-modify", false)]
	[InlineData("a-request-not-yet-classified", false)]
	public void ReadOnlyTransport_AllowsOnlyKnownReads(string requestName, bool allowed)
	{
		using var transport = new DmapiTransport(
			new JokerClientOptions { ApiKey = "k", BaseUrl = BaseUrl, IsReadOnly = true },
			"Test Agent",
			static (log, method, url) => log.LogDmapiRequest(method, url),
			static (log, content) => log.LogDmapiResponse(content));

		var act = () => transport.ThrowIfRefused(requestName);

		if (allowed)
		{
			act.Should().NotThrow();
		}
		else
		{
			act.Should().Throw<InvalidOperationException>();
		}
	}

	public void Dispose()
	{
		_stop.Cancel();
		_listener.Stop();
		_stop.Dispose();
	}

	private async Task ServeAsync()
	{
		try
		{
			while (!_stop.IsCancellationRequested)
			{
				using var tcp = await _listener.AcceptTcpClientAsync(_stop.Token);
				await HandleAsync(tcp);
			}
		}
		catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or SocketException)
		{
			// Listener stopped
		}
	}

	private async Task HandleAsync(TcpClient tcp)
	{
		var stream = tcp.GetStream();
		var buffer = new byte[8192];
		var received = new StringBuilder();
		while (!received.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
		{
			var read = await stream.ReadAsync(buffer, _stop.Token);
			if (read == 0)
			{
				break;
			}

			received.Append(Encoding.UTF8.GetString(buffer, 0, read));
		}

		lock (_requests)
		{
			_requests.Add(received.ToString().Split((char)13)[0]);
		}

		const string body = "Status-Code: 0\nStatus-Text: ok\nResult: ACK\nAuth-Sid: stub-session\n\n";
		var response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n{body}";
		await stream.WriteAsync(Encoding.UTF8.GetBytes(response), _stop.Token);
	}
}
