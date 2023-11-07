using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication;

namespace TagzApp.Providers.AzureQueue;

public class StartAzureQueue : BaseConfigurationProvider, IConfigureProvider
{

	private AzureQueueConfiguration _azureQueueConfiguration;

	public StartAzureQueue(IProviderConfigurationRepository providerConfigurationRepository) : base(providerConfigurationRepository)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		await LoadConfigurationValuesAsync("WEBSITE", cancellationToken);

		services.AddSingleton(_azureQueueConfiguration ?? new());
		services.AddTransient<ISocialMediaProvider, AzureQueueProvider>();
		return services;
	}

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{

		var config = providerConfiguration.ConfigurationSettings;

		if (config != null && config.Any(kv => kv.Key.Equals("QueueConnectionString", StringComparison.InvariantCultureIgnoreCase)))
		{
			_azureQueueConfiguration = new()
			{
				Activated = providerConfiguration.Activated,
				QueueConnectionString = config["QueueConnectionString"] ?? string.Empty
			};
		}

	}

}
