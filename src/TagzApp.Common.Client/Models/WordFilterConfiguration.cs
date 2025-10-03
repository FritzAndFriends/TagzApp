using TagzApp.Common;

namespace TagzApp.Common.Models;

public class WordFilterConfiguration
{
	/// <summary>
	/// Event raised when the WordFilter configuration is updated
	/// </summary>
	public static event EventHandler<WordFilterConfigurationChangedEventArgs>? ConfigurationChanged;

	public const string ConfigurationKey = "wordfilter";

	public bool Enabled { get; set; } = false;

	public string[] BlockedWords { get; set; } = Array.Empty<string>();

	/// <summary>
	/// Load WordFilterConfiguration from the database
	/// </summary>
	/// <param name="config">The configuration provider</param>
	/// <returns>WordFilterConfiguration instance</returns>
	public static async Task<WordFilterConfiguration> LoadFromConfiguration(IConfigureTagzApp config)
	{
		return await config.GetConfigurationById<WordFilterConfiguration>(ConfigurationKey);
	}

	/// <summary>
	/// Save WordFilterConfiguration to the database
	/// </summary>
	/// <param name="configure">The configuration provider</param>
	public async Task SaveConfiguration(IConfigureTagzApp configure)
	{
		await configure.SetConfigurationById(ConfigurationKey, this);
		
		// Raise the configuration changed event
		ConfigurationChanged?.Invoke(this, new WordFilterConfigurationChangedEventArgs(this));
	}
}

/// <summary>
/// Event arguments for WordFilter configuration changes
/// </summary>
public class WordFilterConfigurationChangedEventArgs : EventArgs
{
	public WordFilterConfiguration Configuration { get; }

	public WordFilterConfigurationChangedEventArgs(WordFilterConfiguration configuration)
	{
		Configuration = configuration;
	}
}