using AwesomeAssertions;
using Joker.Api.Models;

namespace Joker.Api.Test;

/// <summary>
/// Tests for DmapiResponse model
/// </summary>
public class DmapiResponseTests
{
	[Fact]
	public void DmapiResponse_DefaultConstructor_InitializesCollections()
	{
		// Act
		var response = new DmapiResponse();

		// Assert
		response.Errors.Should().NotBeNull();
		response.Errors.Should().BeEmpty();
		response.Warnings.Should().NotBeNull();
		response.Warnings.Should().BeEmpty();
		response.Headers.Should().NotBeNull();
		response.Headers.Should().BeEmpty();
	}

	[Fact]
	public void DmapiResponse_IsSuccess_ReturnsTrueWhenStatusCode0AndResultAck()
	{
		// Arrange
		var response = new DmapiResponse
		{
			StatusCode = 0,
			Result = "ACK"
		};

		// Act & Assert
		response.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public void DmapiResponse_IsSuccess_ReturnsFalseWhenStatusCodeNot0()
	{
		// Arrange
		var response = new DmapiResponse
		{
			StatusCode = 1,
			Result = "ACK"
		};

		// Act & Assert
		response.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void DmapiResponse_IsSuccess_ReturnsFalseWhenResultNotAck()
	{
		// Arrange
		var response = new DmapiResponse
		{
			StatusCode = 0,
			Result = "NACK"
		};

		// Act & Assert
		response.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void DmapiResponse_IsSuccess_ReturnsFalseWhenResultIsNull()
	{
		// Arrange
		var response = new DmapiResponse
		{
			StatusCode = 0,
			Result = null
		};

		// Act & Assert
		response.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void DmapiResponse_IsSuccess_IsCaseInsensitiveForResult()
	{
		// Arrange
		var response = new DmapiResponse
		{
			StatusCode = 0,
			Result = "ack" // lowercase
		};

		// Act & Assert
		response.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public void DmapiResponse_Properties_CanBeSet()
	{
		// Arrange & Act
		var response = new DmapiResponse
		{
			AuthSid = "test-auth-sid",
			Uid = "test-uid",
			TrackingId = "test-tracking-id",
			StatusCode = 200,
			StatusText = "OK",
			Result = "ACK",
			ProcId = "test-proc-id",
			AccountBalance = "100.00",
			Body = "test body"
		};

		// Assert
		response.AuthSid.Should().Be("test-auth-sid");
		response.Uid.Should().Be("test-uid");
		response.TrackingId.Should().Be("test-tracking-id");
		Assert.Equal(200, response.StatusCode);
		response.StatusText.Should().Be("OK");
		response.Result.Should().Be("ACK");
		response.ProcId.Should().Be("test-proc-id");
		response.AccountBalance.Should().Be("100.00");
		response.Body.Should().Be("test body");
	}

	[Fact]
	public void DmapiResponse_Errors_CanBeAdded()
	{
		// Arrange
		var response = new DmapiResponse();

		// Act
		response.Errors.Add("Error 1");
		response.Errors.Add("Error 2");

		// Assert
		Assert.Equal(2, response.Errors.Count);
		response.Errors.Should().Contain("Error 1");
		response.Errors.Should().Contain("Error 2");
	}

	[Fact]
	public void DmapiResponse_Warnings_CanBeAdded()
	{
		// Arrange
		var response = new DmapiResponse();

		// Act
		response.Warnings.Add("Warning 1");
		response.Warnings.Add("Warning 2");

		// Assert
		Assert.Equal(2, response.Warnings.Count);
		response.Warnings.Should().Contain("Warning 1");
		response.Warnings.Should().Contain("Warning 2");
	}

	[Fact]
	public void DmapiResponse_Headers_CanBeAdded()
	{
		// Arrange
		var response = new DmapiResponse();

		// Act
		response.Headers["Content-Type"] = "text/plain";
		response.Headers["Content-Length"] = "100";

		// Assert
		Assert.Equal(2, response.Headers.Count);
		response.Headers["Content-Type"].Should().Be("text/plain");
		response.Headers["Content-Length"].Should().Be("100");
	}

	[Fact]
	public void DmapiResponse_Headers_AreCaseInsensitive()
	{
		// Arrange
		var response = new DmapiResponse();

		// Act
		response.Headers["Content-Type"] = "text/plain";

		// Assert
		response.Headers["content-type"].Should().Be("text/plain");
		response.Headers["CONTENT-TYPE"].Should().Be("text/plain");
	}
}
