
using System.Collections.Concurrent;
using Drastic.Tools;
using FishyFlip;
using FishyFlip.Models;
using FishyFlip.Tools;
using Microsoft.Extensions.Logging.Debug;

namespace TagzApp.Providers.Bluesky;

public class BlueskyProvider : ISocialMediaProvider
{

	private (SocialMediaStatus status, string message) _status = (SocialMediaStatus.Unknown, "Not yet started");
	private ATWebSocketProtocol? _AtWebSocketProtocol;

	public string Id => "BLUESKY";

	public string DisplayName => "Bluesky";

	public string Description => "BlueSky social media using the AT Protocol";

	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromMilliseconds(1000);

	private ConcurrentQueue<Content> _messageQueue = new();
	private BlueskyConfiguration _Config;

	private HashSet<Hashtag> _Hashtags = new();
	private ATProtocol _AtProtocol;
	private string? _TheTag;

	public void Dispose()
	{
		_AtWebSocketProtocol?.Dispose();
	}

	public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
	{
		return await configure.GetConfigurationById<BlueskyConfiguration>($"provider-{Id}");
	}

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		if (!_Hashtags.Contains(tag)) _Hashtags.Add(tag);
		if (string.IsNullOrEmpty(_TheTag))
		{
			_TheTag = _Hashtags.FirstOrDefault()?.Text;
		}


		var outMessages = _messageQueue.ToArray();
		for (var i = 0; i < outMessages.Count(); i++)
		{
			_ = _messageQueue.TryDequeue(out _);
		}

		return Task.FromResult(outMessages.AsEnumerable());
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth() => Task.FromResult(_status);

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{
		await configure.SetConfigurationById($"provider-{Id}", providerConfiguration);
	}

	public async Task StartAsync()
	{

		_Config = (await GetConfiguration(ConfigureTagzAppFactory.Current)) as BlueskyConfiguration ?? new BlueskyConfiguration();

		var debugLog = new DebugLoggerProvider();

		_AtProtocol = new ATProtocolBuilder()
			.EnableAutoRenewSession(true)
			.Build();

		// You can set a custom url with WithInstanceUrl
		var atProtocolBuilder = new ATWebSocketProtocolBuilder()
				.WithLogger(debugLog.CreateLogger("BlueskyDebug"));
		_AtWebSocketProtocol = atProtocolBuilder.Build();
		_AtWebSocketProtocol.OnSubscribedRepoMessage += (sender, args) =>
		{
			Task.Run(() => HandleMessageAsync(args.Message)).FireAndForgetSafeAsync();
		};

		await _AtWebSocketProtocol.StartSubscribeReposAsync();

		_status.status = SocialMediaStatus.Healthy;
		_status.message = "Connected to Bluesky";

	}

	private async Task HandleMessageAsync(SubscribeRepoMessage message)
	{

		if (message.Commit is null)
		{
			return;
		}

		var orgId = message.Commit.Repo;

		if (orgId is null)
		{
			return;
		}

		if (message.Record is Post post)
		{

			// TODO: Handle more than 1 hashtag
			if (string.IsNullOrEmpty(_TheTag) || !post.Text!.Contains($"{_TheTag}", StringComparison.InvariantCultureIgnoreCase))
				return;


			var actor = await _AtProtocol.Repo.GetActorAsync(message.Commit.Repo!);
			if (!actor.IsT1)
			{

				var actorRecord = actor.HandleResult();

				// The Actor Did.
				var did = message.Commit.Repo;
				// Commit.Ops are the actions used when creating the message.
				// In this case, it's a create record for the post.
				// The path contains the post action and path, we need the path, so we split to get it.
				var postUrl = $"https://bsky.app/profile/{did}/post/{message.Commit.Ops![0]!.Path!.Split("/").Last()}";
				var repo = (await _AtProtocol.Repo.DescribeRepoAsync(did)).HandleResult();

				_messageQueue.Enqueue(
					new Content
					{
						Author = new Creator
						{
							DisplayName = actor.AsT0?.Value!.DisplayName!,
							UserName = $"@{repo.Handle}",
							ProfileImageUri = new Uri($"https://{_AtProtocol.Options.Url.Host}{Constants.Urls.ATProtoSync.GetBlob}?did={actorRecord.Uri.Did!}&cid={actorRecord.Value.Avatar.Ref.Link}"),
							ProfileUri = new Uri($"https://bsky.app/profile/{did}") // Syntax is like: https://bsky.app/profile/csharpfritz.com
						},
						Provider = Id,
						ProviderId = message.Commit.Commit.Hash.ToString(),
						SourceUri = new Uri(postUrl),
						Timestamp = new DateTimeOffset(post.CreatedAt!.Value).ToUniversalTime(),
						HashtagSought = _TheTag,
						Text = post.Text,
						Type = ContentType.Message
					}
				);

			}
			Console.WriteLine($"Post: {post.Text}");
		}

	}

	public async Task StopAsync()
	{
		if (_AtWebSocketProtocol is null) return;

		await _AtWebSocketProtocol.StopSubscriptionAsync();

		_status.status = SocialMediaStatus.Unhealthy;
		_status.message = "Disconnected from Bluesky";

	}

}
