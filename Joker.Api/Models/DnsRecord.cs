namespace Joker.Api.Models;

/// <summary>
/// Represents a DNS record for SVC operations
/// </summary>
public class DnsRecord
{
	/// <summary>
	/// Gets or sets the record type (A, AAAA, CNAME, MX, TXT, etc.)
	/// </summary>
	public required string Type { get; set; }

	/// <summary>
	/// Gets or sets the label/subdomain (use @ for apex/root domain)
	/// </summary>
	public required string Label { get; set; }

	/// <summary>
	/// Gets or sets the record value/target
	/// </summary>
	public required string Value { get; set; }

	/// <summary>
	/// Gets or sets the TTL (Time To Live) in seconds (optional)
	/// </summary>
	public int? Ttl { get; set; }

	/// <summary>
	/// Gets or sets the priority (for MX records)
	/// </summary>
	public int? Priority { get; set; }

	/// <summary>
	/// Converts the DNS record to Joker zone file format
	/// </summary>
	/// <returns>Zone file formatted string</returns>
	public string ToZoneFormat()
	{
		var parts = new List<string> { Type, Label };

		if (Priority.HasValue)
		{
			parts.Add(Priority.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		parts.Add(Value);

		if (Ttl.HasValue)
		{
			parts.Add(Ttl.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		return string.Join(":", parts);
	}

	/// <summary>
	/// Creates a TXT record
	/// </summary>
	/// <param name="label">The subdomain label</param>
	/// <param name="value">The TXT record value</param>
	/// <returns>DNS TXT record</returns>
	public static DnsRecord CreateTxtRecord(string label, string value) =>
		CreateTxtRecord(label, value, null);

	/// <summary>
	/// Creates a TXT record with TTL
	/// </summary>
	/// <param name="label">The subdomain label</param>
	/// <param name="value">The TXT record value</param>
	/// <param name="ttl">TTL in seconds</param>
	/// <returns>DNS TXT record</returns>
	public static DnsRecord CreateTxtRecord(string label, string value, int? ttl) => new()
	{
		Type = "TXT",
		Label = label,
		Value = value,
		Ttl = ttl
	};

	/// <summary>
	/// Creates an A record
	/// </summary>
	/// <param name="label">The subdomain label</param>
	/// <param name="ipAddress">The IPv4 address</param>
	/// <param name="ttl">Optional TTL</param>
	/// <returns>DNS A record</returns>
	public static DnsRecord CreateARecord(string label, string ipAddress, int? ttl) => new()
	{
		Type = "A",
		Label = label,
		Value = ipAddress,
		Ttl = ttl
	};

	/// <summary>
	/// Creates a CNAME record
	/// </summary>
	/// <param name="label">The subdomain label</param>
	/// <param name="target">The CNAME target</param>
	/// <returns>DNS CNAME record</returns>
	public static DnsRecord CreateCnameRecord(string label, string target) =>
		CreateCnameRecord(label, target, null);

	/// <summary>
	/// Creates a CNAME record with TTL
	/// </summary>
	/// <param name="label">The subdomain label</param>
	/// <param name="target">The CNAME target</param>
	/// <param name="ttl">TTL in seconds</param>
	/// <returns>DNS CNAME record</returns>
	public static DnsRecord CreateCnameRecord(string label, string target, int? ttl) => new()
	{
		Type = "CNAME",
		Label = label,
		Value = target,
		Ttl = ttl
	};
}
