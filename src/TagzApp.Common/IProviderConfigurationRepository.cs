namespace TagzApp.Common;

public interface IProviderConfigurationRepository
{
	Task<ProviderConfiguration?> GetConfigurationSettingsAsync(string name, CancellationToken cancellationToken = default);
	Task<IEnumerable<ProviderConfiguration?>> GetConfigurationSettingsAsync(CancellationToken cancellationToken = default);
	Task SaveConfigurationSettingsAsync(ProviderConfiguration configuration, CancellationToken cancellationToken = default);
}
