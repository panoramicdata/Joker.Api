namespace Joker.Api.Models;

/// <summary>
/// Response from a DMAPI request
/// </summary>
public class DmapiResponse
{
	/// <summary>
	/// Gets or sets the authentication session ID
	/// </summary>
	public string? AuthSid { get; set; }

	/// <summary>
	/// Gets or sets the user ID
	/// </summary>
	public string? Uid { get; set; }

	/// <summary>
	/// Gets or sets the tracking ID
	/// </summary>
	public string? TrackingId { get; set; }

	/// <summary>
	/// Gets or sets the status code (0 = success)
	/// </summary>
	public int StatusCode { get; set; }

	/// <summary>
	/// Gets or sets the status text
	/// </summary>
	public string? StatusText { get; set; }

	/// <summary>
	/// Gets or sets the result (ACK or NACK)
	/// </summary>
	public string? Result { get; set; }

	/// <summary>
	/// Gets or sets the processing ID
	/// </summary>
	public string? ProcId { get; set; }

	/// <summary>
	/// Gets or sets the account balance
	/// </summary>
	public string? AccountBalance { get; set; }

	/// <summary>
	/// Gets or sets error messages
	/// </summary>
	public List<string> Errors { get; set; } = [];

	/// <summary>
	/// Gets or sets warning messages
	/// </summary>
	public List<string> Warnings { get; set; } = [];

	/// <summary>
	/// Gets or sets the response body (data after headers)
	/// </summary>
	public string? Body { get; set; }

	/// <summary>
	/// Gets all headers as a dictionary
	/// </summary>
	public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Indicates if the request was successful
	/// </summary>
	public bool IsSuccess => StatusCode == 0 && Result?.Equals("ACK", StringComparison.OrdinalIgnoreCase) == true;
}
