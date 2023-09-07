// Ignore Spelling: Css

using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace TagzApp.Web.Data;

public class ApplicationConfiguration
{

	internal List<Settings> ChangedSettings = new();
	private string _SiteName = "TagzApp";
	private string _WaterfallHeaderMarkdown = "# Welcome to TagzApp";
	private string _WaterfallHeaderCss = string.Empty;

	[Required, MaxLength(30)]
	public string SiteName {
		get => _SiteName;
		set
		{
			_SiteName = value;
			ChangedSettings.Add(new Settings(SettingsKeys.SiteName, value));
		}
	}

	[Required]
	public string WaterfallHeaderMarkdown {
		get => _WaterfallHeaderMarkdown;
		set
		{
			_WaterfallHeaderMarkdown = value;
			ChangedSettings.Add(new Settings(SettingsKeys.WaterfallHeaderMarkdown, value));
		}
	}

	[Required]
	public string WaterfallHeaderCss {
		get => _WaterfallHeaderCss;
		set {
			_WaterfallHeaderCss = value;
			ChangedSettings.Add(new Settings(SettingsKeys.WaterfallHeaderCss, value));
		}
	}

	public static implicit operator Dictionary<string,string?> (ApplicationConfiguration config)
	{

		return new Dictionary<string, string?>
		{
			{ SettingsKeys.SiteName, config.SiteName },
			{ SettingsKeys.WaterfallHeaderMarkdown, config.WaterfallHeaderMarkdown },
			{ SettingsKeys.WaterfallHeaderCss, config.WaterfallHeaderCss }
		};

	}

	private class SettingsKeys {

		public const string SiteName = "ApplicationConfiguration:SiteName";
		public const string WaterfallHeaderMarkdown = "ApplicationConfiguration:WaterfallHeaderMarkdown";
		public const string WaterfallHeaderCss = "ApplicationConfiguration:WaterfallHeaderCss";

	}

}

public class ApplicationConfigurationRepository
{
	private readonly IConfiguration _Configuration;
	private readonly IOptions<ApplicationConfiguration> _Options;

	public ApplicationConfigurationRepository(IConfiguration configuration, IOptions<ApplicationConfiguration> options)
	{
		_Configuration = configuration;
		_Options = options;
	}

	public async Task SetValues(ApplicationConfiguration config)
	{

		using var ctx = new SecurityContext(_Configuration);

		ctx.Settings.UpdateRange(config.ChangedSettings);
		await ctx.SaveChangesAsync();

	}


}


public class ApplicationConfigurationSource : IConfigurationSource
{
	private readonly IConfiguration _Configuration;

	public ApplicationConfigurationSource(IConfiguration configuration) =>
			_Configuration = configuration;

	public IConfigurationProvider Build(IConfigurationBuilder builder) =>
			new ApplicationConfigurationProvider(_Configuration);
}

public class ApplicationConfigurationProvider : ConfigurationProvider
{
	private readonly IConfiguration _Configuration;

	public ApplicationConfigurationProvider(IConfiguration configuration)
	{
		_Configuration = configuration;
	}

	public override void Load()
	{
		using var dbContext = new SecurityContext(_Configuration);
		dbContext.Database.EnsureCreated();

		Data = dbContext.Settings.Any()
				? dbContext.Settings.ToDictionary<Settings, string, string?>(c => c.Id, c => c.Value, StringComparer.OrdinalIgnoreCase)
				: CreateAndSaveDefaultValues(dbContext);
	}

	static IDictionary<string, string?> CreateAndSaveDefaultValues(
			SecurityContext context)
	{

		Dictionary<string,string?> settings = new ApplicationConfiguration();

		context.Settings.AddRange(
				settings.Select(kvp => new Settings(kvp.Key, kvp.Value))
								.ToArray());

		context.SaveChanges();

		return settings;
	}


	public override void Set(string key, string? value)
	{
		base.Set(key, value);
	}

}
