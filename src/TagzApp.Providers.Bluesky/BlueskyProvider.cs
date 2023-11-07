
using System.Collections.Immutable;
using Drastic.Tools;
using FishyFlip;
using FishyFlip.Models;
using Microsoft.Extensions.Logging.Debug;

namespace TagzApp.Providers.Bluesky;

public class BlueskyProvider : ISocialMediaProvider
{

	private (SocialMediaStatus status, string message) _status = (SocialMediaStatus.Unknown, "Not yet started");
	private ATWebSocketProtocol? _AtProtocol;

	public string Id => "bluesky";

	public string DisplayName => "Bluesky";

	public string Description => "BlueSky social media using the AT Protocol";

	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromMilliseconds(1000);

	private ImmutableQueue<SubscribeRepoMessage> _messageQueue = ImmutableQueue<SubscribeRepoMessage>.Empty;
	private BlueskyConfiguration _Config;

	public void Dispose()
	{
		_AtProtocol?.Dispose();
	}

	public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
	{
		return await configure.GetConfigurationById<BlueskyConfiguration>($"provider-{Id}");
	}

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		return Task.FromResult(Enumerable.Empty<Content>());
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth() => Task.FromResult(_status);

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{
		await configure.SetConfigurationById($"provider-{Id}", providerConfiguration);
	}

	public async Task StartAsync()
	{

		_Config = (await GetConfiguration(ConfigureTagzAppFactory.Current)) as BlueskyConfiguration ?? new BlueskyConfiguration();

		if (_Config.Enabled is false) return;

		var debugLog = new DebugLoggerProvider();

		// You can set a custom url with WithInstanceUrl
		var atProtocolBuilder = new ATWebSocketProtocolBuilder()
				.WithLogger(debugLog.CreateLogger("FishyFlipDebug"));
		_AtProtocol = atProtocolBuilder.Build();

		_AtProtocol.OnSubscribedRepoMessage += (sender, args) =>
		{
			Task.Run(() => HandleMessageAsync(args.Message)).FireAndForgetSafeAsync();
		};

		await _AtProtocol.StartSubscribeReposAsync();

	}

	private void HandleMessageAsync(SubscribeRepoMessage message)
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
			Console.WriteLine($"Post: {post.Text}");
		}

	}

	public async Task StopAsync()
	{
		if (_AtProtocol is null) return;

		await _AtProtocol.StopSubscriptionAsync();
	}

}
