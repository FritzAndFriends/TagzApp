using Microsoft.EntityFrameworkCore;
using Models = TagzApp.Common.Models;

namespace TagzApp.Storage.Postgres;

public class PostgresProviderConfigurationRepository : IProviderConfigurationRepository
{
	private readonly TagzAppContext _Context;

	public PostgresProviderConfigurationRepository(TagzAppContext tagzAppContext)
	{
		_Context = tagzAppContext;
	}

	public async Task<Models.ProviderConfiguration?> GetConfigurationSettingsAsync(string name, CancellationToken cancellation = default)
	{
		var providerConfiguration = await _Context.ProviderConfigurations.FirstOrDefaultAsync(x => x.Name == name);

		return providerConfiguration?.AsProviderConfigurationCommon();
	}

	public async Task<IEnumerable<Models.ProviderConfiguration?>> GetConfigurationSettingsAsync(CancellationToken cancellationToken = default)
	{
		var providerConfigurations = await _Context.ProviderConfigurations.ToListAsync();

		return providerConfigurations?.AsProviderConfigurationsCommon() ?? new List<Common.Models.ProviderConfiguration>();

	}

	public async Task SaveConfigurationSettingsAsync(Models.ProviderConfiguration configuration, CancellationToken cancellationToken = default)
	{
		var entityUpdate = configuration.AsProviderConfiguration();

		if (entityUpdate != null)
		{
			var existingEntity = await _Context.ProviderConfigurations.FindAsync(entityUpdate.Id, cancellationToken);
			if (existingEntity != null)
			{
				_Context.ProviderConfigurations.Entry(existingEntity).State = EntityState.Detached;
				_Context.ProviderConfigurations.Entry(entityUpdate).Property(e => e.ConfigurationSettings).IsModified = true;
				_Context.ProviderConfigurations.Entry(entityUpdate).Property(e => e.Activated).IsModified = true;
			}
			else
			{
				await _Context.ProviderConfigurations.AddAsync(entityUpdate, cancellationToken);
			}
			await _Context.SaveChangesAsync();
		}
	}
}
internal static class ProviderConfigurationTranslations
{
	public static Models.ProviderConfiguration? AsProviderConfigurationCommon(this ProviderConfiguration providerConfiguration)
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

	public static IEnumerable<Models.ProviderConfiguration?>? AsProviderConfigurationsCommon(this IEnumerable<ProviderConfiguration> providerConfigurations)
	{
		return providerConfigurations == null ?
			new List<Common.Models.ProviderConfiguration>() :
			providerConfigurations.Select(x => x.AsProviderConfigurationCommon())
															.Where(y => y != null);
	}

	public static ProviderConfiguration? AsProviderConfiguration(this Models.ProviderConfiguration? providerConfiguration)
	{
		return providerConfiguration == null ? null : new ProviderConfiguration
		{
			Id = providerConfiguration.Id,
			Name = providerConfiguration.Name,
			Activated = providerConfiguration.Activated,
			Description = providerConfiguration.Description,
			ConfigurationSettings = providerConfiguration.ConfigurationSettings
		};
	}
}

