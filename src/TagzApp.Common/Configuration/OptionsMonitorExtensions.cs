using Microsoft.Extensions.Options;

namespace TagzApp.Common.Configuration;

/// <summary>
/// Extension methods for creating configuration utilities for IOptionsMonitor scenarios
/// </summary>
public static class OptionsMonitorExtensions
{
	/// <summary>
	/// Creates a StaticOptionsMonitor from an IOptions instance
	/// </summary>
	/// <typeparam name="T">The configuration type</typeparam>
	/// <param name="options">The IOptions instance to wrap</param>
	/// <returns>A StaticOptionsMonitor that returns the options value</returns>
	public static IOptionsMonitor<T> ToStaticMonitor<T>(this IOptions<T> options) where T : class
	{
		return new StaticOptionsMonitor<T>(options.Value);
	}

	/// <summary>
	/// Creates a StaticOptionsMonitor from a configuration value
	/// </summary>
	/// <typeparam name="T">The configuration type</typeparam>
	/// <param name="value">The configuration value to wrap</param>
	/// <returns>A StaticOptionsMonitor that returns the specified value</returns>
	public static IOptionsMonitor<T> CreateStaticMonitor<T>(T value) where T : class
	{
		return new StaticOptionsMonitor<T>(value);
	}
}
