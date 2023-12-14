
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Drastic.Tools;
using FishyFlip;
using FishyFlip.Models;
using FishyFlip.Tools;
using Microsoft.Extensions.Logging.Debug;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

	private HashSet<Hashtag> _Hashtags = new HashSet<Hashtag>();
	private ATProtocol _AtProtocol;

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

		var outMessages = _messageQueue.ToArray();
		for (var i=0;i<outMessages.Count();i++)
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
			var theTag = _Hashtags.FirstOrDefault()?.Text;
			if (!string.IsNullOrEmpty(theTag) && post.Text!.Contains($" {theTag}", StringComparison.InvariantCultureIgnoreCase))
			{
				var actor = await _AtProtocol.Repo.GetActorAsync(message.Commit.Repo!);
				if (!actor.IsT1)
				{

					_messageQueue.Enqueue(
						new Content {
							Author = new Creator {
								DisplayName = actor.AsT0?.Value!.DisplayName!,
								UserName = actor.AsT0?.Value!.Type,
								ProfileImageUri = new Uri($"https://cdn.bsky.app/img/avatar/plain/{message.Commit.Repo!.ToString().Replace("did:plc:did:plc:", "did:plc:")}/{actor.AsT0?.Value.Avatar.Ref.Link}@jpeg"),
								ProfileUri = new Uri("https://bsky.app") // Syntax is like: https://bsky.app/profile/csharpfritz.com
							},
							Provider = Id,
							ProviderId = message.Commit.Commit.Hash.ToString(),
							SourceUri = new Uri("https://bsky.app/"), // syntax is like:  https://bsky.app/profile/csharpfritz.com/post/3kgj4nty7vp2t
							Timestamp = new DateTimeOffset(post.CreatedAt!.Value).ToUniversalTime(),
							HashtagSought = theTag,
							Text = post.Text,
							Type = ContentType.Message
						}
					);

				}
				Console.WriteLine($"Post: {post.Text}");
			}

		}

	}

	public async Task StopAsync()
	{
		if (_AtWebSocketProtocol is null) return;

		await _AtWebSocketProtocol.StopSubscriptionAsync();
	}

}
