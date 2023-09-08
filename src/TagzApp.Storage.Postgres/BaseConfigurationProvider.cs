using System.Text.Json;

namespace TagzApp.Storage.Postgres;

public abstract class BaseConfigurationProvider
{
	private readonly IProviderConfigurationRepository _ProviderConfigurationRepository;

	public BaseConfigurationProvider(IProviderConfigurationRepository providerConfigurationRepository)
	{
		_ProviderConfigurationRepository = providerConfigurationRepository;
	}

	protected abstract void MapConfigurationValues(JsonDocument configurationSettings);

	protected async Task LoadConfigurationValues(string providerName, CancellationToken cancellationToken = default)
	{
		var providerConfiguration = await _ProviderConfigurationRepository.GetConfigurationSettingsAsync(providerName, cancellationToken);

		if (providerConfiguration != null &&
			providerConfiguration.ConfigurationSettings != null)
		{
			MapConfigurationValues(providerConfiguration.ConfigurationSettings);
		}
	}
}
