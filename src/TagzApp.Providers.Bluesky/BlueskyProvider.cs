using Drastic.Tools;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using FishyFlip.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System.Collections.Concurrent;
using TagzApp.Common.Telemetry;

namespace TagzApp.Providers.Bluesky;

public class BlueskyProvider : ISocialMediaProvider
{

	private (SocialMediaStatus status, string message) _status = (SocialMediaStatus.Unknown, "Not yet started");
	private ATWebSocketProtocol? _AtWebSocketProtocol;

	public string Id => "BLUESKY";

	public static readonly string ConfigurationKey = "provider-BLUESKY";

	public string DisplayName => "Bluesky";

	public string Description => "BlueSky social media using the AT Protocol";

	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromMilliseconds(1000);

	public bool Enabled { get; private set; }

	private ConcurrentQueue<Content> _messageQueue = new();
	private BlueskyConfiguration _Config;

	private HashSet<Hashtag> _Hashtags = new();
	private ATProtocol _AtProtocol;
	private string? _TheTag;

	private readonly ILogger<BlueskyProvider> _Logger;
	private readonly ProviderInstrumentation? _Instrumentation;

	public BlueskyProvider(BlueskyConfiguration configuration, ILogger<BlueskyProvider> logger, ProviderInstrumentation? instrumentation = null)
	{
		Enabled = configuration.Enabled;
		_Config = configuration;
		_Logger = logger;
		_Instrumentation = instrumentation;

		if (!Enabled)
		{
			_status = (SocialMediaStatus.Disabled, "Bluesky is not enabled");
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Disabled);
		}

	}

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

		if (_Instrumentation is not null && outMessages.Any())
		{
			_Logger.LogInformation("Bluesky: Retrieved {Count} new messages", outMessages.Length);
			foreach (var msg in outMessages)
			{
				if (!string.IsNullOrEmpty(msg.Author?.UserName))
				{
					_Instrumentation.AddMessage(Id.ToLowerInvariant(), msg.Author.UserName);
				}
			}
		}

		return Task.FromResult(outMessages.AsEnumerable());
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth() => Task.FromResult(_status);

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{

		await configure.SetConfigurationById($"provider-{Id}", providerConfiguration);

		if (_Config is null) _Config = new();

		if (_Config.Enabled != providerConfiguration.Enabled && _Config.Enabled)
		{
			Enabled = providerConfiguration.Enabled;
			_Config = (BlueskyConfiguration)providerConfiguration;
			_status = (SocialMediaStatus.Disabled, "Bluesky is disabled");
			_Logger.LogInformation("Bluesky: Configuration changed - disabling provider");
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Disabled);
			await StopAsync();
		}
		else if (_Config.Enabled != providerConfiguration.Enabled && !_Config.Enabled)
		{
			Enabled = providerConfiguration.Enabled;
			_Config = (BlueskyConfiguration)providerConfiguration;
			_Logger.LogInformation("Bluesky: Configuration changed - enabling provider");
			await StartAsync();
		}

	}

	public async Task StartAsync()
	{

		//Enabled = _Config.Enabled;

		if (!Enabled)
		{
			_Logger.LogInformation("Bluesky: Not starting - provider is disabled");
			_status.status = SocialMediaStatus.Disabled;
			_status.message = "Bluesky is disabled";
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Disabled);
			return;
		}

		_Logger.LogInformation("Bluesky: Starting connection to AT Protocol");

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

		_Logger.LogInformation("Bluesky: Successfully connected to AT Protocol");
		_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Healthy);

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


			var profileResult = await _AtProtocol.Actor.GetProfileAsync(message.Commit.Repo!);
			if (profileResult.IsT0)
			{

				var profile = profileResult.AsT0;

				// The Actor Did.
				var did = message.Commit.Repo;
				// Commit.Ops are the actions used when creating the message.
				// In this case, it's a create record for the post.
				// The path contains the post action and path, we need the path, so we split to get it.
				var postUrl = $"https://bsky.app/profile/{did}/post/{message.Commit.Ops![0]!.Path!.Split("/").Last()}";
				
				// Construct profile image URL if avatar exists
				var profileImageUri = !string.IsNullOrEmpty(profile.Avatar)
					? new Uri(profile.Avatar)
					: new Uri("https://bsky.app/img/default-avatar.png");

				_messageQueue.Enqueue(
					new Content
					{
						Author = new Creator
						{
							DisplayName = profile.DisplayName ?? profile.Handle?.ToString() ?? did?.ToString() ?? "Unknown",
							UserName = $"@{profile.Handle?.ToString() ?? did?.ToString()}",
							ProfileImageUri = profileImageUri,
							ProfileUri = new Uri($"https://bsky.app/profile/{did}") // Syntax is like: https://bsky.app/profile/csharpfritz.com
						},
						Provider = Id,
						ProviderId = message.Commit?.Commit?.ToString() ?? "",
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

		_Logger.LogInformation("Bluesky: Stopping connection to AT Protocol");

		await _AtWebSocketProtocol.StopSubscriptionAsync();

		_status.status = SocialMediaStatus.Disabled;
		_status.message = "Disconnected from Bluesky";

		_Logger.LogInformation("Bluesky: Disconnected from AT Protocol");
		_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Disabled);

	}

}
