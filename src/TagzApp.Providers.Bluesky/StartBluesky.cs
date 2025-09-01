using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Providers.Bluesky;

public class StartBluesky : IConfigureProvider
{
	private BlueskyConfiguration _BlueskyConfiguration;

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		_BlueskyConfiguration = await ConfigureTagzAppFactory.Current.GetConfigurationById<BlueskyConfiguration>(BlueskyProvider.ConfigurationKey);

		services.AddSingleton(_BlueskyConfiguration ?? new BlueskyConfiguration());

		services.AddTransient<ISocialMediaProvider, BlueskyProvider>();
		return services;
	}
}
