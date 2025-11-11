using AwesomeAssertions;
using Joker.Api.Exceptions;

namespace Joker.Api.Test;

/// <summary>
/// Tests for exception classes
/// </summary>
public class ExceptionTests
{
	[Fact]
	public void JokerApiException_Constructor_WithMessage_SetsMessage()
	{
		// Arrange
		const string message = "Test error message";

		// Act
		var exception = new JokerApiException(message);

		// Assert
		exception.Message.Should().Be(message);
		exception.StatusCode.Should().BeNull();
		exception.ErrorDetails.Should().BeNull();
		exception.InnerException.Should().BeNull();
	}

	[Fact]
	public void JokerApiException_Constructor_WithMessageAndInnerException_SetsProperties()
	{
		// Arrange
		const string message = "Test error message";
		var innerException = new InvalidOperationException("Inner error");

		// Act
		var exception = new JokerApiException(message, innerException);

		// Assert
		exception.Message.Should().Be(message);
		exception.InnerException.Should().BeSameAs(innerException);
		exception.StatusCode.Should().BeNull();
		exception.ErrorDetails.Should().BeNull();
	}

	[Fact]
	public void JokerApiException_Constructor_WithMessageStatusCodeAndErrorDetails_SetsAllProperties()
	{
		// Arrange
		const string message = "Test error message";
		const int statusCode = 500;
		const string errorDetails = "Detailed error information";

		// Act
		var exception = new JokerApiException(message, statusCode, errorDetails);

		// Assert
		exception.Message.Should().Be(message);
		Assert.Equal(statusCode, exception.StatusCode);
		exception.ErrorDetails.Should().Be(errorDetails);
		exception.InnerException.Should().BeNull();
	}

	[Fact]
	public void JokerApiException_Constructor_WithMessageAndStatusCode_SetsPropertiesWithNullDetails()
	{
		// Arrange
		const string message = "Test error message";
		const int statusCode = 400;

		// Act
		var exception = new JokerApiException(message, statusCode);

		// Assert
		exception.Message.Should().Be(message);
		Assert.Equal(statusCode, exception.StatusCode);
		exception.ErrorDetails.Should().BeNull();
	}

	[Fact]
	public void JokerAuthenticationException_Constructor_WithMessage_Sets401StatusCode()
	{
		// Arrange
		const string message = "Authentication failed";

		// Act
		var exception = new JokerAuthenticationException(message);

		// Assert
		exception.Message.Should().Be(message);
		Assert.Equal(401, exception.StatusCode);
		exception.InnerException.Should().BeNull();
	}

	[Fact]
	public void JokerAuthenticationException_Constructor_WithMessageAndInnerException_SetsProperties()
	{
		// Arrange
		const string message = "Authentication failed";
		var innerException = new InvalidOperationException("Inner error");

		// Act
		var exception = new JokerAuthenticationException(message, innerException);

		// Assert
		exception.Message.Should().Be(message);
		exception.InnerException.Should().BeSameAs(innerException);
	}

	[Fact]
	public void JokerNotFoundException_Constructor_WithMessage_Sets404StatusCode()
	{
		// Arrange
		const string message = "Resource not found";

		// Act
		var exception = new JokerNotFoundException(message);

		// Assert
		exception.Message.Should().Be(message);
		Assert.Equal(404, exception.StatusCode);
		exception.InnerException.Should().BeNull();
	}

	[Fact]
	public void JokerNotFoundException_Constructor_WithMessageAndResourceId_IncludesResourceIdInMessage()
	{
		// Arrange
		const string message = "Domain not found";
		const string resourceId = "example.com";

		// Act
		var exception = new JokerNotFoundException(message, resourceId);

		// Assert
		exception.Message.Should().Contain(message);
		exception.Message.Should().Contain(resourceId);
		Assert.Equal(404, exception.StatusCode);
	}
}
