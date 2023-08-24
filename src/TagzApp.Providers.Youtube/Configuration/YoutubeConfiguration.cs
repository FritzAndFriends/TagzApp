using Google.Apis.YouTube.v3;

namespace TagzApp.Providers.Youtube.Configuration;

/// <summary>
/// Defines the Youtube configuration
/// </summary>
public class YoutubeConfiguration
{
	/// <summary>
	/// Declare the section name used
	/// </summary>
	public const string AppSettingsSection = "providers:youtube";

	/// <summary>
	/// YouTube assigned API key
	/// </summary>
	public required string ApiKey { get; set; }

	public SearchResource.ListRequest.SafeSearchEnum SafeSearch { get; set; } = SearchResource.ListRequest.SafeSearchEnum.Moderate;

	public long MaxResults { get; set; } = 50;
}
