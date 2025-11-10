namespace Joker.Api.Exceptions;

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class JokerAuthenticationException : JokerApiException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="JokerAuthenticationException"/> class
	/// </summary>
	/// <param name="message">The error message</param>
	public JokerAuthenticationException(string message)
		: base(message, 401)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerAuthenticationException"/> class
	/// </summary>
	/// <param name="message">The error message</param>
	/// <param name="innerException">The inner exception</param>
	public JokerAuthenticationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
