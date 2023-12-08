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
	public string[] Keys => ["Timeout", "DefaultHeaders", "UseHttp2"];

	public TwitterConfiguration()
	{
		BaseAddress = new Uri("https://api.twitter.com");
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
