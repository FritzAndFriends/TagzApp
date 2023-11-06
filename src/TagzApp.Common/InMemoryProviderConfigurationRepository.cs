using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace TagzApp.Common;
public class InMemoryProviderConfigurationRepository : IProviderConfigurationRepository
{
	private readonly IConfiguration _Configuration;

	public InMemoryProviderConfigurationRepository(IConfiguration configuration)
	{
		_Configuration = configuration;
	}

	public async Task<ProviderConfiguration> GetConfigurationSettingsAsync(string name, CancellationToken cancellationToken = default)
	{
		var appSettingsSection = $"providers:{name.ToLower()}";
		var providerConfig = new ProviderConfiguration
		{
			Name = name,
		};

		var configSettings = _Configuration.GetSection(appSettingsSection).GetChildren();

		foreach (var configSetting in configSettings)
		{

			switch (configSetting.Key.ToLowerInvariant())
			{
				case "activated":
					providerConfig.Activated = bool.Parse(configSetting.Value ?? "false");
					continue;
					break;
				case "description":
					providerConfig.Description = configSetting.Value ?? string.Empty;
					continue;
					break;
				case "defaultheaders":
					if (configSetting.GetChildren().Count() == 0 || configSetting.Value is string)
					{
						providerConfig.ConfigurationSettings!.Add(configSetting.Key, configSetting.Value ?? string.Empty);
						continue;
					}
					var headerDict = new Dictionary<string, string>();
					// iterate through the children of the DefaultHeaders section and add to the dictionary
					foreach (var header in configSetting.GetChildren())
					{
						headerDict.Add(header.Key, header.Value ?? string.Empty);
					}
					providerConfig.ConfigurationSettings!.Add("DefaultHeaders", JsonSerializer.Serialize(headerDict));
					continue;
					break;

			}

			providerConfig.ConfigurationSettings!.Add(configSetting.Key, configSetting.Value ?? string.Empty);

		}

		return providerConfig;
	}

	public Task<IEnumerable<ProviderConfiguration?>> GetConfigurationSettingsAsync(CancellationToken cancellationToken = default)
	{

		var providersSections = _Configuration.GetSection("providers").GetChildren();
		var providerConfigurations = new List<ProviderConfiguration?>();
		foreach (var section in providersSections)
		{

			var providerConfig = new ProviderConfiguration
			{
				Name = section.Key,
			};

			var configSettings = section.GetChildren();

			foreach (var configSetting in configSettings)
			{
				if (configSetting.Key == "Activated")
				{
					providerConfig.Activated = bool.Parse(configSetting.Value ?? "false");
					continue;
				}

				if (configSetting.Key == "Description")
				{
					providerConfig.Description = configSetting.Value ?? string.Empty;
					continue;
				}

				providerConfig.ConfigurationSettings!.Add(configSetting.Key, configSetting.Value ?? string.Empty);
			}

			providerConfigurations.Add(providerConfig);

		}

		return Task.FromResult(providerConfigurations.AsEnumerable());

	}

	public Task SaveConfigurationSettingsAsync(ProviderConfiguration configuration, CancellationToken cancellationToken = default)
	{

		// do nothing - we can't save values back to IConfiguration
		return Task.CompletedTask;

	}
}
