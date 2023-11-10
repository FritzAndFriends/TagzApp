using Microsoft.EntityFrameworkCore;
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

	public Task SetValue(string key, string value)
	{

		try
		{
			using var ctx = new TagzAppContext(_Configuration);

			var exists = ctx.Settings.Any(s => s.Id == key);
			var setting = new Settings(key, value);
			if (exists)
			{
				ctx.Settings.Update(setting);
			}
			else
			{
				ctx.Settings.Add(setting);
			}

			ctx.SaveChanges();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error saving setting: {ex.Message}");
			throw new Exception("Error saving setting", ex);
		}

		return Task.CompletedTask;


	}

	public async Task SetValues(Common.Models.ApplicationConfiguration config)
	{

		try
		{
			using var ctx = new TagzAppContext(_Configuration);

			var settingsIds = config.ChangedSettings.Select(s => s.Id).ToList();
			var missingSettings = ctx.Settings.AsNoTracking().Where(s => !settingsIds.Any(cs => cs == s.Id)).ToList();
			if (missingSettings.Any())
			{
				ctx.Settings.AddRange(missingSettings);
			}

			var changedSettings = ctx.Settings.AsNoTracking().Where(s => settingsIds.Any(cs => cs == s.Id)).ToList();
			if (changedSettings.Any())
			{
				ctx.Settings.UpdateRange(config.ChangedSettings.Where(c => changedSettings.Any(h => h.Id == c.Id)).ToArray());
			}

			await ctx.SaveChangesAsync();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error saving settings: {ex.Message}");
			throw new Exception("Error saving settings", ex);
		}
	}




}

