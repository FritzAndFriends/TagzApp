using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Communication.Extensions;

/// <summary>
/// Extension methods for provider configuration dependency injection setup
/// </summary>
public static class ProviderConfigurationExtensions
{
	/// <summary>
	/// Configures a provider configuration type in the DI container
	/// </summary>
	/// <typeparam name="TConfig">The configuration type</typeparam>
	/// <param name="services">The service collection</param>
	/// <param name="configureTagzApp">The configuration provider</param>
	/// <returns>The configured service collection</returns>
	public static async Task<IServiceCollection> AddProviderConfiguration<TConfig>(
		this IServiceCollection services,
		IConfigureTagzApp configureTagzApp)
		where TConfig : BaseProviderConfiguration<TConfig>, new()
	{
		// Load the configuration instance
		var configInstance = await BaseProviderConfiguration<TConfig>.CreateFromConfigurationAsync<TConfig>(configureTagzApp);

		// Register it as a singleton
		services.AddSingleton<TConfig>(configInstance);

		return services;
	}

	/// <summary>
	/// Configures a provider configuration type in the DI container with a custom setup action
	/// </summary>
	/// <typeparam name="TConfig">The configuration type</typeparam>
	/// <param name="services">The service collection</param>
	/// <param name="configureAction">Custom configuration action</param>
	/// <returns>The configured service collection</returns>
	public static IServiceCollection AddProviderConfiguration<TConfig>(
		this IServiceCollection services,
		Action<TConfig> configureAction)
		where TConfig : BaseProviderConfiguration<TConfig>, new()
	{
		services.Configure<TConfig>(configureAction);
		return services;
	}
}
