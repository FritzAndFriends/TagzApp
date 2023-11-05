namespace TagzApp.Common;

/// <summary>
/// Defines the interface for a Social Media Provider that delivers content to TagzApp
/// </summary>
public interface ISocialMediaProvider
{

	/// <summary>
	/// Unique identifier for the provider
	/// </summary>
	string Id { get; }

	/// <summary>
	/// Display name for the provider
	/// </summary>
	string DisplayName { get; }

	virtual string DllName => DisplayName;

	/// <summary>
	/// Provider description
	/// </summary>
	string Description { get; }

	/// <summary>
	/// How frequently new content should be retrieved from the provider
	/// </summary>
	TimeSpan NewContentRetrievalFrequency { get; }

	/// <summary>
	/// Get a collection of content for a given hashtag
	/// </summary>
	/// <param name="tag">Tag to search for</param>
	/// <param name="since">Datestamp to search for content since</param>
	/// <returns></returns>
	Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since);

	/// <summary>
	/// Report the health of the provider
	/// </summary>
	/// <returns></returns>
	Task<(SocialMediaStatus Status, string Message)> GetHealth();

	/// <summary>
	/// Start the provider
	/// </summary>
	/// <returns></returns>
	Task StartAsync();

}
