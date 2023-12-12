using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Twitter.Configuration;

namespace TagzApp.Providers.Twitter;

public class StartTwitter : IConfigureProvider
{
	private const string _DisplayName = "Twitter";
	private TwitterConfiguration? _TwitterConfiguration;

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		_TwitterConfiguration = await ConfigureTagzAppFactory.Current.GetConfigurationById<TwitterConfiguration>(TwitterConfiguration.AppSettingsSection);

		services.AddSingleton(_TwitterConfiguration ?? new TwitterConfiguration());
		services.AddHttpClient<ISocialMediaProvider, TwitterProvider, TwitterConfiguration>(_TwitterConfiguration ?? new TwitterConfiguration());
		services.AddTransient<ISocialMediaProvider, TwitterProvider>();
		return services;

	}

}
