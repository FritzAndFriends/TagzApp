using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace TagzApp.Providers.Mastodon;

internal class MastodonProvider : ISocialMediaProvider
{

	private readonly HttpClient _HttpClient;
	private readonly ILogger _Logger;

	public MastodonProvider(HttpClient httpClient, ILogger<MastodonProvider> logger)
	{
		_HttpClient = httpClient;
		_Logger = logger;
	}

	public string Id => "MASTODON";
	public string DisplayName => "Mastodon";

	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(20);

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		var targetUri = FormatUri(tag);

		Message[]? messages = null;
		try
		{
			messages = await _HttpClient.GetFromJsonAsync<Message[]>(targetUri);
		} catch (Exception ex)
		{

			_Logger.LogError(ex, "Error getting content from Mastodon");
			return Enumerable.Empty<Content>();

		}

		if (messages is null || (!messages?.Any() ?? true)) 		{
			return Enumerable.Empty<Content>();
		}

		return messages!.Select(m => new Content
		{
			Provider = Id,
			ProviderId = m.id,
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
			Text = m.content,
			HashtagSought = tag.Text
		});

	}

	private Uri FormatUri(Hashtag tag)
	{
		return new Uri($"/api/v1/timelines/tag/{HttpUtility.UrlEncode(tag.Text)}?limit=20", UriKind.Relative);
	}
}



