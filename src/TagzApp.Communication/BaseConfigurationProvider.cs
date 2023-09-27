namespace TagzApp.Communication;

public abstract class BaseConfigurationProvider
{
	private readonly IProviderConfigurationRepository _ProviderConfigurationRepository;

	public BaseConfigurationProvider(IProviderConfigurationRepository providerConfigurationRepository)
	{
		_ProviderConfigurationRepository = providerConfigurationRepository;
	}

	protected abstract void MapConfigurationValues(ProviderConfiguration providerConfiguration);

	protected async Task LoadConfigurationValuesAsync(string providerName, CancellationToken cancellationToken = default)
	{
		var providerConfiguration = await _ProviderConfigurationRepository.GetConfigurationSettingsAsync(providerName, cancellationToken);

		if (providerConfiguration != null)
		{
			MapConfigurationValues(providerConfiguration);
		}
	}
}
