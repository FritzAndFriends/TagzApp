using Microsoft.Extensions.Logging;

using System.IO.Compression;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Web;

using TagzApp.Providers.Twitter.Configuration;
using TagzApp.Providers.Twitter.Models;

namespace TagzApp.Providers.Twitter;

public class TwitterProvider : ISocialMediaProvider, IHasNewestId
{
	private readonly HttpClient _HttpClient;
	private readonly TwitterConfiguration _Configuration;
	private readonly ILogger<TwitterProvider> _Logger;
	private const string _SearchFields = "created_at,author_id,entities";
	private const int _SearchMaxResults = 100;
	private const string _SearchExpansions = "author_id,attachments.media_keys";

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	public string Id => "TWITTER";
	public string DisplayName => "Twitter";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(30);

	public static int MaxContentPerHashtag { get; set; } = 100;

	public string NewestId { get; set; } = string.Empty;

	public string Description { get; init; } = "Twitter is a service for friends, family, and coworkers to communicate and stay connected through the exchange of quick, frequent messages";

	public TwitterProvider(IHttpClientFactory httpClientFactory, ILogger<TwitterProvider> logger,
		TwitterConfiguration configuration)
	{
		_HttpClient = httpClientFactory.CreateClient(nameof(TwitterProvider));
		_Configuration = configuration;
		_Logger = logger;

		if (!string.IsNullOrWhiteSpace(configuration.Description))
		{
			Description = configuration.Description;
		}
	}
	// TODO: Check CS1998: Async method lacks 'await' operators and will run synchronously
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async Task<IEnumerable<Content>> GetContentForHashtag(Common.Models.Hashtag tag, DateTimeOffset since)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = "OK";

		var tweetQuery = "#" + tag.Text.ToLowerInvariant().TrimStart('#') + " -is:retweet";
		var sinceTerm = string.IsNullOrEmpty(NewestId) ? "" : $"&since_id={NewestId}";

		var targetUri = FormatUri(tweetQuery, sinceTerm);

		TwitterData recentTweets = new();
		try
		{

			if (_Configuration.Activated)
			{

				//var response = await _HttpClient.GetAsync(targetUri);
				//var rawText = await response.Content.ReadAsStringAsync();

				recentTweets = await _HttpClient.GetFromJsonAsync<TwitterData>(targetUri);
				NewestId = recentTweets.meta.newest_id ?? NewestId;

			}
			else
			{

				_Status = SocialMediaStatus.Degraded;
				_StatusMessage = "Twitter provider is not activated - returning sample tweets";

				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "TagzApp.Providers.Twitter.Models.SampleTweets.json.gz";
				string sampleJson = string.Empty;

				using var stream = assembly.GetManifestResourceStream(resourceName);
				if (stream is not null)
				{
					using var unzip = new GZipStream(stream, CompressionMode.Decompress);
					var embeddedTweets = JsonSerializer.Deserialize<TwitterData>(unzip);
					if (embeddedTweets is not null) recentTweets = embeddedTweets;
				}

			}

		}
		catch (Exception ex)
		{

			Console.WriteLine($"Error retrieving tweets: {ex.Message}");

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Error retrieving tweets: {ex.Message}";

			_Logger.LogError(ex, $"Error retrieving tweets");

		}

		var outTweets = ConvertToContent(recentTweets, tag);

		return outTweets;

	}

	private IEnumerable<Content> ConvertToContent(TwitterData? recentTweets, Common.Models.Hashtag tag)
	{

		if (recentTweets is null) return Enumerable.Empty<Content>();
		if ((recentTweets.data?.Length ?? 0) == 0) return Enumerable.Empty<Content>();

		var outContent = new List<Content>();

		foreach (var t in recentTweets.data!)
		{

			var author = recentTweets.includes.users.FirstOrDefault(u => u.id == t.author_id);
			if (author is null) continue;

			try
			{

				var formattedText = t.text;
				if (t.entities?.urls is not null)
				{
					foreach (var url in t.entities.urls)
					{
						formattedText = formattedText.Replace(url.url, $"""<a href="{url.expanded_url}" target="_blank">{url.display_url}</a>""");
					}
				}

				formattedText = formattedText.Replace("\n", "<br/>");

				var c = new Content
				{
					Provider = Id,
					ProviderId = t.id,
					Author = new Creator
					{
						DisplayName = author.name,
						UserName = $"@{author.username}",
						ProviderId = "TWITTER",
						ProfileImageUri = new(author.profile_image_url),
						ProfileUri = new($"https://twitter.com/{author.username}")
					},
					SourceUri = new Uri($"https://twitter.com/{author.username}/status/{t.id}"),
					Text = formattedText,
					Timestamp = new DateTimeOffset(t.created_at),
					HashtagSought = tag.Text,
					Type = ContentType.Message
				};

				if (t.entities?.urls?.Any(u => u.images != null) ?? false)
				{

					var thisUrl = t.entities.urls?.FirstOrDefault(u => u.images != null);
					var thisImage = thisUrl?.images?.FirstOrDefault();

					if (thisImage is not null)
					{

						c.PreviewCard = new Card
						{
							AltText = thisUrl?.title ?? "",
							Height = thisImage.height,
							Width = thisImage.width,
							ImageUri = new Uri(thisImage.url),
						};
					}

				}
				else if (t.attachments?.media_keys?.Any() ?? false)
				{

					var thisMediaKey = t.attachments.media_keys.First();
					var thisMedia = recentTweets.includes.media.FirstOrDefault(u => u.media_key == thisMediaKey);
					if (thisMedia is not null)
					{

						c.PreviewCard = new Card
						{
							AltText = thisMedia.alt_text ?? "",
							Height = thisMedia.height ?? 0,
							Width = thisMedia.width ?? 0,
							ImageUri = new Uri(thisMedia.preview_image_url ?? "about:blank"),
						};

					}

				}

				outContent.Add(c);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error formatting tweet ('{t.text}'): ${ex.Message}");
				_Logger.LogError(ex, $"Error formatting tweet: {t.text}");

				_Status = SocialMediaStatus.Degraded;
				_StatusMessage = $"Error formatting tweet: {t.text}";

			}

		}

		return outContent;

	}

	private static Uri FormatUri(string tweetQuery, string sinceTerm)
	{
		var query = string.Concat(
			"query=", HttpUtility.UrlEncode(tweetQuery),
			sinceTerm,
			"&max_results=", _SearchMaxResults,
			"&tweet.fields=", _SearchFields,
			"&user.fields=profile_image_url",
			"&media.fields=preview_image_url",
			"&expansions=", _SearchExpansions);

		return new Uri($"/2/tweets/search/recent?{query}", UriKind.Relative);
	}

	public Task StartAsync()
	{
		return Task.CompletedTask;
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth()
	{

		return Task.FromResult((_Status, _StatusMessage));

	}
}
