using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Xml.Linq;

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

		return providerConfiguration?.AsProviderConfigurationCommon();
	}

	public async Task<IEnumerable<Common.Models.ProviderConfiguration?>> GetConfigurationSettingsAsync(CancellationToken cancellationToken = default)
	{
		var providerConfigurations = await _Context.ProviderConfigurations.ToListAsync();

		return providerConfigurations?.AsProviderConfigurationsCommon() ?? new List<Common.Models.ProviderConfiguration>();

	}
}
internal static class ProviderConfigurationTranslations
{
	public static Common.Models.ProviderConfiguration? AsProviderConfigurationCommon(this ProviderConfiguration providerConfiguration)
	{
		return providerConfiguration == null ? null : new Common.Models.ProviderConfiguration
		{
			Id = providerConfiguration.Id,
			Name = providerConfiguration.Name,
			Activated = providerConfiguration.Activated,
			Description = providerConfiguration.Description,
			ConfigurationSettings = providerConfiguration.ConfigurationSettings
		};
	}

	public static IEnumerable<Common.Models.ProviderConfiguration?>? AsProviderConfigurationsCommon(this IEnumerable<ProviderConfiguration> providerConfigurations)
	{
		return providerConfigurations == null ?
			new List<Common.Models.ProviderConfiguration>() :
			providerConfigurations.Select(x => x.AsProviderConfigurationCommon())
															.Where(y => y!= null);
	}
}

