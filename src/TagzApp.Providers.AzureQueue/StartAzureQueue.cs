using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication;

namespace TagzApp.Providers.AzureQueue;

public class StartAzureQueue : IConfigureProvider
{

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		var config = await ConfigureTagzAppFactory.Current.GetConfigurationById<AzureQueueConfiguration>($"provider:AzureQueue");

		services.AddSingleton(config ?? new());
		services.AddTransient<ISocialMediaProvider, AzureQueueProvider>();
		return services;

	}

}
