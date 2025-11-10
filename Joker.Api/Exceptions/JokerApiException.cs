namespace Joker.Api.Exceptions;

/// <summary>
/// Base exception for all Joker API exceptions
/// </summary>
public class JokerApiException : Exception
{
	/// <summary>
	/// Gets the HTTP status code
	/// </summary>
	public int? StatusCode { get; }

	/// <summary>
	/// Gets the error details from the API response
	/// </summary>
	public string? ErrorDetails { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerApiException"/> class
	/// </summary>
	/// <param name="message">The error message</param>
	public JokerApiException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerApiException"/> class
	/// </summary>
	/// <param name="message">The error message</param>
	/// <param name="innerException">The inner exception</param>
	public JokerApiException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerApiException"/> class
	/// </summary>
	/// <param name="message">The error message</param>
	/// <param name="statusCode">The HTTP status code</param>
	/// <param name="errorDetails">The error details from the API</param>
	public JokerApiException(string message, int statusCode, string? errorDetails = null)
		: base(message)
	{
		StatusCode = statusCode;
		ErrorDetails = errorDetails;
	}
}
