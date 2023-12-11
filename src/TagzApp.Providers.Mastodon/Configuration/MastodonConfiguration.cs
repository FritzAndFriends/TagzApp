using System.Text.Json.Serialization;
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
	public const string AppSettingsSection = "provider-mastodon";

	[JsonIgnore]
	public string Description => "Search the Mastodon federated services for a specified hashtag";

	[JsonIgnore]
	public string Name => "Mastodon";

	public bool Enabled { get; set; }

	[JsonIgnore]
	public string[] Keys => ["BaseAddress", "Timeout", "DefaultHeaders", "UseHttp2"];

	public MastodonConfiguration()
	{
		BaseAddress = new Uri("https://mastodon.social");
	}

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"BaseAddress" => BaseAddress?.ToString() ?? string.Empty,
			"Timeout" => Timeout.ToString(),
			"DefaultHeaders" => DefaultHeaders?.Serialize() ?? string.Empty,
			"UseHttp2" => UseHttp2.ToString(),
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{
		switch (key)
		{
			case "BaseAddress":
				BaseAddress = new Uri(value);
				break;
			case "Timeout":
				Timeout = TimeSpan.Parse(value);
				break;
			case "DefaultHeaders":
				DefaultHeaders = DeserializeHeaders(value);
				break;
			case "UseHttp2":
				UseHttp2 = bool.Parse(value);
				break;
			default:
				throw new NotImplementedException();

		}
	}
}
