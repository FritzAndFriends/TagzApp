using TagzApp.Communication.Configuration;

namespace TagzApp.Providers.Mastodon.Configuration;

/// <summary>
/// Defines the Mastondon configuration
/// </summary>
internal class MastodonConfiguration : HttpClientOptions
{
	/// <summary>
	/// Declare the section name used
	/// </summary>
	public const string AppSettingsSection = "providers:mastodon";

	public string Description { get; set; } = string.Empty;

	public MastodonConfiguration()
	{
		BaseAddress = new Uri("https://mastodon.social");
	}
}
