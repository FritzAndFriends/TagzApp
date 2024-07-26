using TagzApp.Communication.Configuration;
using TagzApp.Providers.Blazot.Constants;

namespace TagzApp.Providers.Blazot.Configuration;

public class BlazotConfiguration : HttpClientOptions, IProviderConfiguration
{
	/// <summary>
	/// Declare the section name used.
	/// </summary>
	public const string AppSettingsSection = BlazotConstants.ProviderId;

	public bool Enabled { get; set; }

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
	public int WindowSeconds { get; set; } = 300;

	/// <summary>
	/// The number of requests the account allows within the window.
	/// </summary>
	public int WindowRequests { get; set; } = 5;

	/// <summary>
	/// Provider Description
	/// </summary>
	public string Description => BlazotConstants.Description;

	public string Name => BlazotConstants.DisplayName;

	public string[] Keys => [nameof(Enabled), nameof(ApiKey), nameof(SecretAuthKey), nameof(WindowSeconds), nameof(WindowRequests), nameof(BaseAddress), nameof(Timeout), nameof(DefaultHeaders), nameof(UseHttp2)];

	public BlazotConfiguration()
	{
		BaseAddress = new Uri(BlazotConstants.BaseAddress);
	}

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			nameof(ApiKey) => ApiKey,
			nameof(SecretAuthKey) => SecretAuthKey,
			nameof(WindowSeconds) => WindowSeconds.ToString(),
			nameof(WindowRequests) => WindowRequests.ToString(),
			nameof(BaseAddress) => BaseAddress?.ToString() ?? string.Empty,
			nameof(Timeout) => Timeout.ToString(),
			nameof(DefaultHeaders) => DefaultHeaders?.Serialize() ?? string.Empty,
			nameof(UseHttp2) => UseHttp2.ToString(),
			nameof(Enabled) => Enabled.ToString(),
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{

		// Set the property with the same name as the key submitted and don't use reflection
		switch (key)
		{
			case nameof(ApiKey):
				ApiKey = value;
				break;
			case nameof(SecretAuthKey):
				SecretAuthKey = value;
				break;
			case nameof(WindowSeconds):
				WindowSeconds = int.Parse(value);
				break;
			case nameof(WindowRequests):
				WindowRequests = int.Parse(value);
				break;
			case nameof(BaseAddress):
				BaseAddress = new Uri(value);
				break;
			case nameof(Timeout):
				Timeout = TimeSpan.Parse(value);
				break;
			case nameof(DefaultHeaders):
				DefaultHeaders = DeserializeHeaders(value);
				break;
			case nameof(UseHttp2):
				UseHttp2 = bool.Parse(value);
				break;
			case nameof(Enabled):
				Enabled = bool.Parse(value);
				break;
		}
	}
}
