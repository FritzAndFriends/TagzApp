namespace TagzApp.Providers.Mastodon;

internal class MastodonProvider : ISocialMediaProvider
{
	public string Id => "MASTODON";
	public string DisplayName => "Mastodon";

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		throw new NotImplementedException();
	}
}



