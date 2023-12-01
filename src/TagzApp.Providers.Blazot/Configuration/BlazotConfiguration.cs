using TagzApp.Communication.Configuration;

namespace TagzApp.Providers.Blazot.Configuration;

public class BlazotConfiguration : HttpClientOptions
{
	/// <summary>
	/// Declare the section name used.
	/// </summary>
	public const string AppSettingsSection = "providers:blazot";

	public bool Enabled { get; set; } = false;

	/// <summary>
	/// Blazot issued API Key.
	/// </summary>
	public string ApiKey { get; set; } = string.Empty;

	/// <summary>
	/// Blazot issued Secret Auth Key. This is only needed when making a request to get the access token at the auth endpoint.
	/// </summary>
	public string SecretAuthKey { get; set; } = string.Empty;

	/// <summary>
	/// The number of seconds in the rate limit window.
	/// </summary>
	public int WindowSeconds { get; set; } = 60;

	/// <summary>
	/// The number of requests the account allows within the window.
	/// </summary>
	public int WindowRequests { get; set; }

	/// <summary>
	/// Provider Description
	/// </summary>
	public string Description { get; set; }

	public BlazotConfiguration()
	{
		BaseAddress = new Uri("https://api.blazot.com");
	}

}
