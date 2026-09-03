using System.Globalization;

namespace Joker.Api;

/// <summary>
/// Collects the parameters of a DMAPI request, skipping those the caller did not supply.
/// </summary>
internal sealed class DmapiParameters
{
	private readonly Dictionary<string, string> _values = [];

	/// <summary>
	/// The collected parameters
	/// </summary>
	internal IReadOnlyDictionary<string, string> Values => _values;

	/// <summary>
	/// Sets a parameter
	/// </summary>
	internal DmapiParameters Set(string name, string value)
	{
		_values[name] = value;
		return this;
	}

	/// <summary>
	/// Sets a parameter to its invariant decimal representation
	/// </summary>
	internal DmapiParameters Set(string name, int value)
		=> Set(name, value.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	/// Sets a parameter, unless the value is null or blank
	/// </summary>
	internal DmapiParameters SetIfPresent(string name, string? value)
		=> string.IsNullOrWhiteSpace(value) ? this : Set(name, value);

	/// <summary>
	/// Sets a parameter, unless the value is null
	/// </summary>
	internal DmapiParameters SetIfPresent(string name, int? value)
		=> value.HasValue ? Set(name, value.Value) : this;

	/// <summary>
	/// Sets a parameter to the DMAPI's "1" flag value, unless the flag is not set
	/// </summary>
	internal DmapiParameters SetIfEnabled(string name, bool enabled)
		=> enabled ? Set(name, "1") : this;
}
