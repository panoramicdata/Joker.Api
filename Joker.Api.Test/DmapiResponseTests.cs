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
		Assert.NotNull(response.Errors);
		Assert.Empty(response.Errors);
		Assert.NotNull(response.Warnings);
		Assert.Empty(response.Warnings);
		Assert.NotNull(response.Headers);
		Assert.Empty(response.Headers);
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
		Assert.True(response.IsSuccess);
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
		Assert.False(response.IsSuccess);
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
		Assert.False(response.IsSuccess);
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
		Assert.False(response.IsSuccess);
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
		Assert.True(response.IsSuccess);
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
		Assert.Equal("test-auth-sid", response.AuthSid);
		Assert.Equal("test-uid", response.Uid);
		Assert.Equal("test-tracking-id", response.TrackingId);
		Assert.Equal(200, response.StatusCode);
		Assert.Equal("OK", response.StatusText);
		Assert.Equal("ACK", response.Result);
		Assert.Equal("test-proc-id", response.ProcId);
		Assert.Equal("100.00", response.AccountBalance);
		Assert.Equal("test body", response.Body);
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
		Assert.Contains("Error 1", response.Errors);
		Assert.Contains("Error 2", response.Errors);
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
		Assert.Contains("Warning 1", response.Warnings);
		Assert.Contains("Warning 2", response.Warnings);
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
		Assert.Equal("text/plain", response.Headers["Content-Type"]);
		Assert.Equal("100", response.Headers["Content-Length"]);
	}

	[Fact]
	public void DmapiResponse_Headers_AreCaseInsensitive()
	{
		// Arrange
		var response = new DmapiResponse();

		// Act
		response.Headers["Content-Type"] = "text/plain";

		// Assert
		Assert.Equal("text/plain", response.Headers["content-type"]);
		Assert.Equal("text/plain", response.Headers["CONTENT-TYPE"]);
	}
}
