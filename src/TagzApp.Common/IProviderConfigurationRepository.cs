namespace TagzApp.Common;

public interface IProviderConfigurationRepository
{
	Task<ProviderConfiguration?> GetConfigurationSettingsAsync(string name, CancellationToken cancellationToken = default);
}
