using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

internal class PostgresProviderConfigurationRepository : IProviderConfigurationRepository
{
	private readonly TagzAppContext _Context;

	public PostgresProviderConfigurationRepository(TagzAppContext tagzAppContext)
	{
		_Context = tagzAppContext;
	}

	public async Task<Common.Models.ProviderConfiguration> GetConfigurationSettingsAsync(string name, CancellationToken cancellation = default)
	{
		var configSettings = await _Context.ProviderConfigurations.FirstOrDefaultAsync(x => x.Name == name);
		//TODO: Map to another freaking model
		throw new NotImplementedException();
	}
}
