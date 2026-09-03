namespace Joker.Api;

/// <summary>
/// Builds request URLs for the DMAPI text protocol.
/// </summary>
internal static class DmapiRequestUrlBuilder
{
	/// <summary>
	/// Builds the relative request URL for a DMAPI request name and its parameters.
	/// </summary>
	/// <param name="requestName">The request name (e.g., "login", "query-domain-list")</param>
	/// <param name="parameters">Request parameters, which may be empty</param>
	/// <returns>The relative request URL, including the query string when parameters are supplied</returns>
	internal static string Build(string requestName, IReadOnlyDictionary<string, string> parameters)
	{
		var url = $"/request/{requestName}";

		if (parameters.Count == 0)
		{
			return url;
		}

		var queryParams = parameters
			.Select(param => $"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");

		return $"{url}?{string.Join("&", queryParams)}";
	}
}
