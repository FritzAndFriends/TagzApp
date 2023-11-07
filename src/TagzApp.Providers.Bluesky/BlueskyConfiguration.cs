namespace TagzApp.Providers.Bluesky;

public class BlueskyConfiguration : IProviderConfiguration
{
	public string Name => "Bluesky";

	public string Description => "The Bluesky social network served by the AT Protocol";

	public bool Enabled { get; set; }

	public string[] Keys => new string[] { };

	public string GetConfigurationByKey(string key)
	{
		if (key == "Enabled")
		{
			return Enabled.ToString();
		}

		return string.Empty;

	}

	public void SetConfigurationByKey(string key, string value)
	{
		if (key == "Enabled")
		{
			Enabled = bool.Parse(value);
		}
	}
}
