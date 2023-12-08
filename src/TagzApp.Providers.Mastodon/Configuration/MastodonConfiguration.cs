using TagzApp.Communication.Configuration;

namespace TagzApp.Providers.Mastodon.Configuration;

/// <summary>
/// Defines the Mastodon configuration
/// </summary>
internal class MastodonConfiguration : HttpClientOptions, IProviderConfiguration
{
	/// <summary>
	/// Declare the section name used
	/// </summary>
	public const string AppSettingsSection = "providers:mastodon";

	public string Description => "Search the Mastodon federated services for a specified hashtag";

	public string Name => "Mastodon";
	public bool Enabled { get; set; }

	public MastodonConfiguration()
	{
		BaseAddress = new Uri("https://mastodon.social");
	}
}
