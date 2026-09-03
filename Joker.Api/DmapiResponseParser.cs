using Joker.Api.Models;

namespace Joker.Api;

/// <summary>
/// Parses responses returned by the DMAPI text protocol.
/// </summary>
internal static class DmapiResponseParser
{
	internal static DmapiResponse Parse(string content)
	{
		var response = new DmapiResponse();
		var bodyStarted = false;
		var bodyLines = new List<string>();

		foreach (var line in content.Split('\n'))
		{
			var trimmedLine = line.TrimEnd('\r');

			if (string.IsNullOrWhiteSpace(trimmedLine))
			{
				bodyStarted = true;
				continue;
			}

			if (bodyStarted)
			{
				bodyLines.Add(trimmedLine);
			}
			else
			{
				ParseHeaderLine(trimmedLine, response);
			}
		}

		if (bodyLines.Count > 0)
		{
			response.Body = string.Join("\n", bodyLines);
		}

		return response;
	}

	private static void ParseHeaderLine(string line, DmapiResponse response)
	{
		var colonIndex = line.IndexOf(':');
		if (colonIndex <= 0)
		{
			return;
		}

		var headerName = line[..colonIndex].Trim();
		var headerValue = line[(colonIndex + 1)..].Trim();

		response.Headers[headerName] = headerValue;
		response.MapHeader(headerName, headerValue);
	}
}
