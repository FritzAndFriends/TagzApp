using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace TagzApp.Blazor.Components.Pages;

public partial class FirstStartConfiguration
{

	IConfigureTagzApp CurrentConfig = ConfigureTagzAppFactory.Current;

	public async Task OnFormValidate()
	{


		// Grab the ConfigureTagzAppFactory and set the values that were submitted
		await ConfigureTagzAppFactory.SetConfigurationProvider(Config.Provider, Config.ConnectionString);

		if (Config.ConfigurationType.Equals("basic", StringComparison.InvariantCultureIgnoreCase))
		{

			Config.SecurityProvider = Config.Provider;
			Config.ContentProvider = Config.Provider;

			Config.SecurityConnectionString = Config.ConnectionString;
			Config.ContentConnectionString = Config.ConnectionString;

		}

		// Configure the Security and Content providers
		var currentProvider = ConfigureTagzAppFactory.Current;
		var connectionSettings = new ConnectionSettings
		{
			SecurityProvider = Config.SecurityProvider,
			SecurityConnectionString = Config.SecurityConnectionString,
			ContentProvider = Config.ContentProvider,
			ContentConnectionString = Config.ContentConnectionString
		};
		await currentProvider.SetConfigurationById(ConnectionSettings.ConfigurationKey, connectionSettings);

		Program.Restart();

		NavigationManager.NavigateTo("/");


	}

	protected override void OnInitialized()
	{
		Config = new();
		base.OnInitialized();
	}

	void PrecalculateBasicConnectionString(ChangeEventArgs args)
	{

		if (Config.Provider.Equals("sqlite", StringComparison.InvariantCultureIgnoreCase))
		{
			Config.ConnectionString = @"Data Source=tagzapp.db;";
		}

	}


}

public class FirstStartConfig
{

	public string ConfigurationType { get; set; } = "basic";

	private string _Provider;
	[Required]
	public string Provider
	{
		get { return _Provider; }
		set
		{
			_Provider = value;
			if (ConfigurationType.Equals("basic", StringComparison.InvariantCultureIgnoreCase) &&
				value.Equals("sqlite", StringComparison.InvariantCultureIgnoreCase)
			)
			{
				ConnectionString = @"Data Source=tagzapp.db;";
			}

		}
	}

	[Required]
	public string ConnectionString { get; set; }

	public string ContentProvider { get; set; }

	public string? ContentConnectionString { get; set; }

	public string SecurityProvider { get; set; }

	public string? SecurityConnectionString { get; set; }


}
