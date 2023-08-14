using TagzApp.Communication.Configuration;

namespace TagzApp.Providers.Twitter.Configuration;

/// <summary>
/// Defines the Twitter configuration
/// </summary>
public class TwitterConfiguration : HttpClientOptions
{
	/// <summary>
	/// Declare the section name used
	/// </summary>
	public const string AppSettingsSection = "providers:twitter";

	/// <summary>
	/// Twitter issued API Key for the service
	/// </summary>
	public string ApiKey { get; set; } = string.Empty;

	/// <summary>
	/// Twitter issued API Secret Key for the service
	/// </summary>
	public string ApiSecretKey { get; set; } = string.Empty;

	/// <summary>
	/// Access token for Twitter
	/// </summary>
	public string AccessToken { get; set; } = string.Empty;

	/// <summary>
	/// Access token secret for Twitter
	/// </summary>
	public string AccessTokenSecret { get; set; } = string.Empty;
}
