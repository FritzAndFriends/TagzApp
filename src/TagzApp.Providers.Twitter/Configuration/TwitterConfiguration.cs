using TagzApp.Communication.Configuration;

namespace TagzApp.Providers.Twitter.Configuration;

/// <summary>
/// Defines the Twitter configuration
/// </summary>
public class TwitterConfiguration : HttpClientOptions, IProviderConfiguration
{

	/// <summary>
	/// Declare the section name used
	/// </summary>
	public const string AppSettingsSection = "providers:twitter";

	public string Description => "Search the X (formerly Twitter) service for a specified hashtag";
	public string Name => "X (formerly Twitter)";
	public bool Enabled { get; set; }

	public TwitterConfiguration()
	{
		BaseAddress = new Uri("https://api.twitter.com");
	}
}
