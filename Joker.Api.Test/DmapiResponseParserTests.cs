using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="DmapiResponseParser"/>
/// </summary>
public class DmapiResponseParserTests
{
	[Fact]
	public void Parse_WithFullResponse_PopulatesAllFields()
	{
		// Arrange
		const string content =
			"auth-sid: SID-1234\n" +
			"uid: 42\n" +
			"tracking-id: TRK-9\n" +
			"status-code: 0\n" +
			"status-text: success\n" +
			"result: ACK\n" +
			"proc-id: PROC-7\n" +
			"account-balance: 99.99\n" +
			"error: something failed\n" +
			"warning: watch out\n" +
			"custom-header: custom-value\n" +
			"\n" +
			"body line one\n" +
			"body line two\n";

		// Act
		var response = DmapiResponseParser.Parse(content);

		// Assert
		response.AuthSid.Should().Be("SID-1234");
		response.Uid.Should().Be("42");
		response.TrackingId.Should().Be("TRK-9");
		response.StatusCode.Should().Be(0);
		response.StatusText.Should().Be("success");
		response.Result.Should().Be("ACK");
		response.ProcId.Should().Be("PROC-7");
		response.AccountBalance.Should().Be("99.99");
		response.Errors.Should().ContainSingle().Which.Should().Be("something failed");
		response.Warnings.Should().ContainSingle().Which.Should().Be("watch out");
		response.Headers["custom-header"].Should().Be("custom-value");
		response.Body.Should().Be("body line one\nbody line two");
	}

	[Fact]
	public void Parse_WithCrlfLineEndings_StripsCarriageReturns()
	{
		// Arrange
		const string content = "result: ACK\r\n\r\nbody\r\n";

		// Act
		var response = DmapiResponseParser.Parse(content);

		// Assert
		response.Result.Should().Be("ACK");
		response.Body.Should().Be("body");
	}

	[Fact]
	public void Parse_WithLineBeforeColon_SkipsIt()
	{
		// Arrange
		const string content = "no colon here\nresult: ACK\n";

		// Act
		var response = DmapiResponseParser.Parse(content);

		// Assert
		response.Result.Should().Be("ACK");
	}

	[Fact]
	public void Parse_WithLineStartingWithColon_SkipsIt()
	{
		// Arrange
		const string content = ":no-name\nresult: ACK\n";

		// Act
		var response = DmapiResponseParser.Parse(content);

		// Assert
		response.Headers.Should().NotContainKey(":no-name");
	}

	[Fact]
	public void Parse_WithEmptyContent_ReturnsEmptyResponse()
	{
		// Act
		var response = DmapiResponseParser.Parse("");

		// Assert
		response.Headers.Should().BeEmpty();
		response.Body.Should().BeNull();
		response.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void Parse_WithOnlyBody_ReturnsBodyOnly()
	{
		// Arrange
		const string content = "\njust a body line\n";

		// Act
		var response = DmapiResponseParser.Parse(content);

		// Assert
		response.Headers.Should().BeEmpty();
		response.Body.Should().Be("just a body line");
	}

	[Fact]
	public void Parse_WithUnknownHeader_StoresInHeadersDictionary()
	{
		// Arrange
		const string content = "unknown-key: some-value\n";

		// Act
		var response = DmapiResponseParser.Parse(content);

		// Assert
		response.Headers["unknown-key"].Should().Be("some-value");
		response.AuthSid.Should().BeNull();
	}
}
