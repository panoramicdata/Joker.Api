namespace Joker.Api;

/// <summary>
/// Base class for the Joker clients, providing the dispose pattern they share
/// </summary>
public abstract class JokerClientBase : IDisposable
{
	private bool _disposed;

	/// <summary>
	/// Releases the resources held by the client. Overrides do not need to guard against
	/// being called twice, as <see cref="Dispose()"/> only calls this once.
	/// </summary>
	/// <param name="disposing">Whether to dispose managed resources</param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>
	/// Disposes the client and releases resources
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		Dispose(true);
		_disposed = true;

		GC.SuppressFinalize(this);
	}
}
