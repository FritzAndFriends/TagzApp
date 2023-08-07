namespace TagzApp.Providers.Youtube.Configuration;

/// <summary>
/// Defines the Youtube configuration
/// </summary>
public class YoutubeConfiguration
{
	/// <summary>
	/// Declare the section name used
	/// </summary>
	public const string AppSettingsSection = "providers:youtube";

	/// <summary>
	/// YouTube assigned API key
	/// </summary>
	public required string ApiKey { get; set; }
}
