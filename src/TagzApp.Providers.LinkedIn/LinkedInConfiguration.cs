using System.Text.Json.Serialization;

namespace TagzApp.Providers.LinkedIn;

public class LinkedInConfiguration : IProviderConfiguration
{
	public const string AppSettingsSection = "provider-linkedin";

	[JsonIgnore]
	public string Name => "LinkedIn";

	[JsonIgnore]
	public string Description => "Search LinkedIn for posts containing a specified hashtag";

	public bool Enabled { get; set; }

	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public string AccessToken { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public string TokenExpiresAt { get; set; } = string.Empty;
	public int PollingIntervalMinutes { get; set; } = 5;
	public int DailyCallBudget { get; set; } = 100;

	[JsonIgnore]
	public string[] Keys => ["ClientId", "ClientSecret", "AccessToken", "RefreshToken", "TokenExpiresAt", "PollingIntervalMinutes", "DailyCallBudget"];

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ClientId" => ClientId,
			"ClientSecret" => ClientSecret,
			"AccessToken" => AccessToken,
			"RefreshToken" => RefreshToken,
			"TokenExpiresAt" => TokenExpiresAt,
			"PollingIntervalMinutes" => PollingIntervalMinutes.ToString(),
			"DailyCallBudget" => DailyCallBudget.ToString(),
			"Enabled" => Enabled.ToString(),
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{
		switch (key)
		{
			case "ClientId":
				ClientId = value;
				break;
			case "ClientSecret":
				ClientSecret = value;
				break;
			case "AccessToken":
				AccessToken = value;
				break;
			case "RefreshToken":
				RefreshToken = value;
				break;
			case "TokenExpiresAt":
				TokenExpiresAt = value;
				break;
			case "PollingIntervalMinutes":
				PollingIntervalMinutes = int.TryParse(value, out var interval) ? Math.Max(interval, 5) : 5;
				break;
			case "DailyCallBudget":
				DailyCallBudget = int.TryParse(value, out var budget) ? Math.Max(budget, 10) : 100;
				break;
			case "Enabled":
				Enabled = bool.TryParse(value, out var enabled) && enabled;
				break;
			default:
				throw new NotImplementedException();
		}
	}
}
