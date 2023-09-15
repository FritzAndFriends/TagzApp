using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

internal class PostgresProviderConfigurationRepository : IProviderConfigurationRepository
{
	private readonly TagzAppContext _Context;

	public PostgresProviderConfigurationRepository(TagzAppContext tagzAppContext)
	{
		_Context = tagzAppContext;
	}

	public async Task<Common.Models.ProviderConfiguration?> GetConfigurationSettingsAsync(string name, CancellationToken cancellation = default)
	{
		var providerConfiguration = await _Context.ProviderConfigurations.FirstOrDefaultAsync(x => x.Name == name);

		return providerConfiguration == null ? null : new Common.Models.ProviderConfiguration
		{
			Id = providerConfiguration.Id,
			Name = name,
			Activated = providerConfiguration.Activated,
			Description = providerConfiguration.Description,
			ConfigurationSettings = providerConfiguration.ConfigurationSettings
		};
	}
}
