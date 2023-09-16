using Microsoft.Extensions.Configuration;

namespace TagzApp.Storage.Postgres.ApplicationConfiguration;

public class ConfigurationSource : IConfigurationSource
{
	private readonly IConfiguration _Configuration;

	public ConfigurationSource(IConfiguration configuration) =>
			_Configuration = configuration;

	public IConfigurationProvider Build(IConfigurationBuilder builder) =>
			new ApplicationConfigurationProvider(_Configuration);
}

