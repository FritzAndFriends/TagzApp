using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Blazot.Configuration;
using TagzApp.Providers.Blazot.Constants;
using TagzApp.Providers.Blazot.Converters;
using TagzApp.Providers.Blazot.Events;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Services;

namespace TagzApp.Providers.Blazot;

public class StartBlazot : IConfigureProvider
{
	private string _DisplayName => BlazotConstants.DisplayName;
	private BlazotConfiguration? _Configuration;

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		_Configuration = await ConfigureTagzAppFactory.Current.GetConfigurationById<BlazotConfiguration>(BlazotConfiguration.AppSettingsSection);

		services.AddSingleton(_Configuration ?? new BlazotConfiguration());
		services.AddHttpClient<ISocialMediaProvider, BlazotProvider, BlazotConfiguration>(_Configuration ?? new());
		services.AddTransient<ISocialMediaProvider, BlazotProvider>();
		services.AddSingleton<IContentConverter, ContentConverter>();
		services.AddSingleton<ITransmissionsService, HashtagTransmissionsService>();
		services.AddSingleton<IAuthService, AuthService>();
		services.AddSingleton<IAuthEvents, AuthEvents>();
		return services;
	}

}
