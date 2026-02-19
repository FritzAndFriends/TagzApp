using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication.Extensions;

namespace TagzApp.Providers.LinkedIn;

public class StartLinkedIn : IConfigureProvider
{
	private const string _DisplayName = "LinkedIn";
	private LinkedInConfiguration? _linkedInConfiguration;

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		_linkedInConfiguration = await ConfigureTagzAppFactory.Current.GetConfigurationById<LinkedInConfiguration>(LinkedInConfiguration.AppSettingsSection);

		services.AddSingleton(_linkedInConfiguration ?? new());
		services.AddTransient<ISocialMediaProvider, LinkedInProvider>();
		return services;
	}
}
