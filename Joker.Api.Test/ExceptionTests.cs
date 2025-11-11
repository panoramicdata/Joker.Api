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
		Assert.Equal(message, exception.Message);
		Assert.Null(exception.StatusCode);
		Assert.Null(exception.ErrorDetails);
		Assert.Null(exception.InnerException);
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
		Assert.Equal(message, exception.Message);
		Assert.Same(innerException, exception.InnerException);
		Assert.Null(exception.StatusCode);
		Assert.Null(exception.ErrorDetails);
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
		Assert.Equal(message, exception.Message);
		Assert.Equal(statusCode, exception.StatusCode);
		Assert.Equal(errorDetails, exception.ErrorDetails);
		Assert.Null(exception.InnerException);
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
		Assert.Equal(message, exception.Message);
		Assert.Equal(statusCode, exception.StatusCode);
		Assert.Null(exception.ErrorDetails);
	}

	[Fact]
	public void JokerAuthenticationException_Constructor_WithMessage_Sets401StatusCode()
	{
		// Arrange
		const string message = "Authentication failed";

		// Act
		var exception = new JokerAuthenticationException(message);

		// Assert
		Assert.Equal(message, exception.Message);
		Assert.Equal(401, exception.StatusCode);
		Assert.Null(exception.InnerException);
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
		Assert.Equal(message, exception.Message);
		Assert.Same(innerException, exception.InnerException);
	}

	[Fact]
	public void JokerNotFoundException_Constructor_WithMessage_Sets404StatusCode()
	{
		// Arrange
		const string message = "Resource not found";

		// Act
		var exception = new JokerNotFoundException(message);

		// Assert
		Assert.Equal(message, exception.Message);
		Assert.Equal(404, exception.StatusCode);
		Assert.Null(exception.InnerException);
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
		Assert.Contains(message, exception.Message);
		Assert.Contains(resourceId, exception.Message);
		Assert.Equal(404, exception.StatusCode);
	}
}
