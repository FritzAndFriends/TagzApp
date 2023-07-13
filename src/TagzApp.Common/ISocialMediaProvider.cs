namespace TagzApp.Common;

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

	/// <summary>
	/// Get a collection of content for a given hashtag
	/// </summary>
	/// <param name="tag">Tag to search for</param>
	/// <param name="since">Datestamp to search for content since</param>
	/// <returns></returns>
	Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since);



}