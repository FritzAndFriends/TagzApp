using Microsoft.Extensions.Configuration;

namespace TagzApp.Storage.Postgres.ApplicationConfiguration;

public class ApplicationConfigurationProvider : ConfigurationProvider
{
	private readonly IConfiguration _Configuration;

	public ApplicationConfigurationProvider(IConfiguration configuration)
	{
		_Configuration = configuration;
	}

	public override void Load()
	{
		using var dbContext = new TagzAppContext(_Configuration);
		//dbContext.Database.EnsureCreated();

		Data = dbContext.Settings.Any()
				? dbContext.Settings.ToDictionary(c => c.Id, c => c.Value, StringComparer.OrdinalIgnoreCase)
				: CreateAndSaveDefaultValues(dbContext);

	}

	IDictionary<string, string?> CreateAndSaveDefaultValues(
			TagzAppContext context)
	{

		Dictionary<string, string?> settings = new Common.Models.ApplicationConfiguration();

		context.Settings.AddRange(
				settings.Select(kvp => new Settings(kvp.Key, kvp.Value))
								.ToArray());

		try
		{
			context.SaveChanges();
		}
		catch
		{
			// Fail silently... this should only happen at startup on the first load of the application
		}

		return settings;
	}

	public void Reload()
	{
		Load();
		OnReload();
	}
}


