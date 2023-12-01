using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Mastodon.Configuration;

namespace TagzApp.Providers.Mastodon;

public class StartMastodon : IConfigureProvider
{
	private const string _DisplayName = "Mastodon";
	private MastodonConfiguration? _MastodonConfiguration;

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		_MastodonConfiguration = await ConfigureTagzAppFactory.Current.GetConfigurationById<MastodonConfiguration>(MastodonConfiguration.AppSettingsSection);

		services.AddSingleton(_MastodonConfiguration ?? new());
		services.AddHttpClient<ISocialMediaProvider, MastodonProvider, MastodonConfiguration>(_MastodonConfiguration ?? new MastodonConfiguration());
		services.AddTransient<ISocialMediaProvider, MastodonProvider>();
		return services;
	}

}
