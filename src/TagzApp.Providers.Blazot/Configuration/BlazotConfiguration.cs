using TagzApp.Communication.Configuration;

namespace TagzApp.Providers.Blazot.Configuration;

public class BlazotConfiguration : HttpClientOptions, IProviderConfiguration
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
	public string Description => "Interact with the Blazot social media service";
	public string Name => "Blazot";

	public string[] Keys => ["ApiKey", "SecretAuthKey", "WindowSeconds", "WindowRequests", "BaseAddress", "Timeout", "DefaultHeaders", "UseHttp2"];

	public BlazotConfiguration()
	{
		BaseAddress = new Uri("https://api.blazot.com");
	}

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ApiKey" => ApiKey,
			"SecretAuthKey" => SecretAuthKey,
			"WindowSeconds" => WindowSeconds.ToString(),
			"WindowRequests" => WindowRequests.ToString(),
			"BaseAddress" => BaseAddress?.ToString() ?? string.Empty,
			"Timeout" => Timeout.ToString(),
			"DefaultHeaders" => DefaultHeaders?.Serialize() ?? string.Empty,
			"UseHttp2" => UseHttp2.ToString(),
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{

		// Set the property with the same name as the key submitted and don't use reflection
		switch (key)
		{
			case "ApiKey":
				ApiKey = value;
				break;
			case "SecretAuthKey":
				SecretAuthKey = value;
				break;
			case "WindowSeconds":
				WindowSeconds = int.Parse(value);
				break;
			case "WindowRequests":
				WindowRequests = int.Parse(value);
				break;
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
		}


	}
}
