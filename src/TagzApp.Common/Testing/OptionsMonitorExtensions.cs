using Microsoft.Extensions.Options;

namespace TagzApp.Common.Testing;

/// <summary>
/// Extension methods for creating testing utilities for IOptionsMonitor scenarios
/// </summary>
public static class OptionsMonitorExtensions
{
	/// <summary>
	/// Creates a StaticOptionsMonitor from an IOptions instance for testing scenarios
	/// </summary>
	/// <typeparam name="T">The configuration type</typeparam>
	/// <param name="options">The IOptions instance to wrap</param>
	/// <returns>A StaticOptionsMonitor that returns the options value</returns>
	public static IOptionsMonitor<T> ToStaticMonitor<T>(this IOptions<T> options) where T : class
	{
		return new StaticOptionsMonitor<T>(options.Value);
	}

	/// <summary>
	/// Creates a StaticOptionsMonitor from a configuration value for testing scenarios
	/// </summary>
	/// <typeparam name="T">The configuration type</typeparam>
	/// <param name="value">The configuration value to wrap</param>
	/// <returns>A StaticOptionsMonitor that returns the specified value</returns>
	public static IOptionsMonitor<T> CreateStaticMonitor<T>(T value) where T : class
	{
		return new StaticOptionsMonitor<T>(value);
	}
}
