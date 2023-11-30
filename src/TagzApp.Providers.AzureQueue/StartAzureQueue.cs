using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication;

namespace TagzApp.Providers.AzureQueue;

public class StartAzureQueue : BaseConfigurationProvider<AzureQueueConfiguration>, IConfigureProvider
{

	public StartAzureQueue(IConfigureTagzApp configureTagzApp) : base(configureTagzApp)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		var config = await LoadConfigurationValuesAsync("WEBSITE");

		services.AddSingleton(config ?? new());
		services.AddTransient<ISocialMediaProvider, AzureQueueProvider>();
		return services;

	}

}
