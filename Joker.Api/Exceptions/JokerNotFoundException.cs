namespace Joker.Api.Exceptions;

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class JokerNotFoundException : JokerApiException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="JokerNotFoundException"/> class
	/// </summary>
	/// <param name="message">The error message</param>
	public JokerNotFoundException(string message)
		: base(message, 404)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JokerNotFoundException"/> class
	/// </summary>
	/// <param name="message">The error message</param>
	/// <param name="resourceId">The ID of the resource that was not found</param>
	public JokerNotFoundException(string message, string resourceId)
		: base($"{message} (Resource ID: {resourceId})", 404)
	{
	}
}
