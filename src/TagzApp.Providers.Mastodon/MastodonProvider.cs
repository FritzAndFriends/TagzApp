using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace TagzApp.Providers.Mastodon;

internal class MastodonProvider : ISocialMediaProvider
{

	private readonly HttpClient _HttpClient;

	public MastodonProvider(HttpClient httpClient)
	{
		_HttpClient = httpClient;
	}

	public string Id => "MASTODON";
	public string DisplayName => "Mastodon";

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		var targetUri = FormatUri(tag);

		var messages = await _HttpClient.GetFromJsonAsync<Message[]>(targetUri);

		if (messages is null || (!messages?.Any() ?? true)) 		{
			return Enumerable.Empty<Content>();
		}

		return messages!.Select(m => new Content
		{
			ProviderId = Id,
			Type = ContentType.Message,
			Timestamp = m.created_at,
			SourceUri = new Uri(m.uri),
			Author = new Creator {
				DisplayName = m.account!.display_name,
				UserName = m.account.acct,
				ProviderId = Id,
				ProfileImageUri = new Uri(m.account.avatar_static),
				ProfileUri = new Uri(m.account.url)
			},
			Text = m.content
		});

	}

	private Uri FormatUri(Hashtag tag)
	{
		return new Uri($"/api/v1/timelines/tag/{tag.Text}?limit=2", UriKind.Relative);
	}
}



