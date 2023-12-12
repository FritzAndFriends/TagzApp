using Google.Apis.YouTube.v3;

namespace TagzApp.Providers.Youtube.Configuration;

/// <summary>
/// Defines the Youtube configuration
/// </summary>
public class YoutubeConfiguration : IProviderConfiguration
{
	/// <summary>
	/// Declare the section name used
	/// </summary>
	public const string AppSettingsSection = "providers:youtube";

	/// <summary>
	/// YouTube assigned API key
	/// </summary>
	public string ApiKey { get; set; } = string.Empty;

	public SearchResource.ListRequest.SafeSearchEnum SafeSearch { get; set; } = SearchResource.ListRequest.SafeSearchEnum.Moderate;

	public long MaxResults { get; set; } = 50;
	public string Name => "YouTube";
	public string Description => "Search YouTube video descriptions for a hashtag";
	public bool Enabled { get; set; }
	public string[] Keys => ["ApiKey", "MaxResults"];

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ApiKey" => ApiKey,
			"MaxResults" => MaxResults.ToString(),
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{

		switch (key)
		{

			case "ApiKey":
				ApiKey = value;
				break;
			case "MaxResults":
				MaxResults = long.Parse(value);
				break;
			default:
				throw new NotImplementedException();
		}

	}
}
