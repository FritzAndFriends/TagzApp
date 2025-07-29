using Microsoft.Extensions.Options;

namespace TagzApp.Common.Configuration;

/// <summary>
/// A static implementation of IOptionsMonitor{T} that returns a fixed configuration value.
/// Useful for testing scenarios, development environments, or cases where reactive configuration updates are not needed.
/// </summary>
/// <typeparam name="T">The configuration type</typeparam>
public class StaticOptionsMonitor<T> : IOptionsMonitor<T> where T : class
{
	private readonly T _Value;

	/// <summary>
	/// Creates a new StaticOptionsMonitor with the specified configuration value
	/// </summary>
	/// <param name="value">The configuration value to return</param>
	public StaticOptionsMonitor(T value)
	{
		_Value = value;
	}

	/// <summary>
	/// Gets the current configuration value
	/// </summary>
	public T CurrentValue => _Value;

	/// <summary>
	/// Gets the configuration value for the specified name (ignores name parameter)
	/// </summary>
	/// <param name="name">The name parameter (ignored for static configurations)</param>
	/// <returns>The static configuration value</returns>
	public T Get(string? name) => _Value;

	/// <summary>
	/// Registers a listener for configuration changes (no-op for static configurations)
	/// </summary>
	/// <param name="listener">The change listener (ignored for static configurations)</param>
	/// <returns>null (no change subscription for static configurations)</returns>
	public IDisposable? OnChange(Action<T, string?> listener) => null;
}
