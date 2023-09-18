using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace TagzApp.Storage.Postgres.ApplicationConfiguration;

internal class Repository : IApplicationConfigurationRepository
{
	private readonly IConfiguration _Configuration;
	private readonly IOptions<Common.Models.ApplicationConfiguration> _Options;

	public Repository(IConfiguration configuration, IOptions<Common.Models.ApplicationConfiguration> options)
	{
		_Configuration = configuration;
		_Options = options;
	}

	public async Task SetValues(Common.Models.ApplicationConfiguration config)
	{

		using var ctx = new TagzAppContext(_Configuration);

		ctx.Settings.UpdateRange(config.ChangedSettings);
		await ctx.SaveChangesAsync();

	}


}

