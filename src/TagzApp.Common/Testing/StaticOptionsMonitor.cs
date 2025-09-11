using Microsoft.Extensions.Options;

namespace TagzApp.Common.Testing;

/// <summary>
/// A static implementation of IOptionsMonitor{T} for testing scenarios.
/// Returns the same configuration value throughout the test without reactive change notifications.
/// </summary>
/// <typeparam name="T">The configuration type</typeparam>
public class StaticOptionsMonitor<T> : IOptionsMonitor<T> where T : class
{
	private readonly T _value;

	/// <summary>
	/// Creates a new StaticOptionsMonitor with the specified configuration value
	/// </summary>
	/// <param name="value">The configuration value to return</param>
	public StaticOptionsMonitor(T value)
	{
		_value = value;
	}

	/// <summary>
	/// Gets the current configuration value
	/// </summary>
	public T CurrentValue => _value;

	/// <summary>
	/// Gets the configuration value for the specified name (ignores name parameter)
	/// </summary>
	/// <param name="name">The name parameter (ignored in testing scenarios)</param>
	/// <returns>The static configuration value</returns>
	public T Get(string? name) => _value;

	/// <summary>
	/// Registers a listener for configuration changes (no-op in testing scenarios)
	/// </summary>
	/// <param name="listener">The change listener (ignored in testing scenarios)</param>
	/// <returns>null (no change subscription in testing scenarios)</returns>
	public IDisposable? OnChange(Action<T, string?> listener) => null;
}
