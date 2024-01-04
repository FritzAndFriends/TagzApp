using System.Collections.Concurrent;
using TagzApp.Communication;

namespace TagzApp.Blazor.Services;

public class InMemoryMessagingService : BaseProviderManager, IMessagingService
{

	private InMemoryContentMessaging _Service = default;

	private readonly ILogger<InMemoryMessagingService> _Logger;
	private readonly INotifyNewMessages _NotifyNewMessages;

	public InMemoryMessagingService(
		IConfiguration configuration,
		ILogger<InMemoryMessagingService> logger,
		INotifyNewMessages notifyNewMessages,
		IEnumerable<ISocialMediaProvider>? socialMediaProviders = null
	) : base(logger, socialMediaProviders)
	{
		_Logger = logger;
		_NotifyNewMessages = notifyNewMessages;
	}

	/// <summary>
	/// A collection of the tags and the content found for them.
	/// </summary>
	private readonly Dictionary<string, ConcurrentBag<Content>> _Content = new();

	#region Hosted Service Implementation

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await InitProviders();
		_Service = new InMemoryContentMessaging();
		_Service.StartProviders(Providers, cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_Service.Dispose();
		return Task.CompletedTask;
	}

	#endregion

	public Task AddHashtagToWatch(string tag)
	{

		if (!_Content.ContainsKey(Hashtag.ClearFormatting(tag)))
		{   // Check hashtag without #
			_Content.Add(Hashtag.ClearFormatting(tag), new ConcurrentBag<Content>());
		}

		_Service.SubscribeToContent(new Hashtag() { Text = tag },
			c =>
			{
				_Content[c.HashtagSought.TrimStart('#')].Add(c);
				_NotifyNewMessages.NotifyNewContent(Hashtag.ClearFormatting(c.HashtagSought), c);
				//_Logger.LogInformation($"Message found for tag '{c.HashtagSought}': {c.Text}");
			});

		return Task.CompletedTask;

	}

	public Task<IEnumerable<Content>> GetExistingContentForTag(string tag)
	{

		if (!_Service._LoadedContent.ContainsKey(tag.TrimStart('#').ToLowerInvariant())) return Task.FromResult(Enumerable.Empty<Content>());

		return Task.FromResult(_Service._LoadedContent[tag.TrimStart('#').ToLowerInvariant()].AsEnumerable());

	}

	public async Task<Content?> GetContentByIds(string provider, string providerId)
	{

		return _Content.First().Value
			.FirstOrDefault(c => c.Provider == provider && c.ProviderId == providerId);

	}

	public Task<IEnumerable<Content>> GetApprovedContentByTag(string tag)
	{

		return Task.FromResult(Enumerable.Empty<Content>());

	}

	public Task<IEnumerable<(Content, ModerationAction)>> GetContentByTagForModeration(string tag)
	{
		throw new NotImplementedException();
	}

	public string GetLatestProviderIdByTagAndProvider(string tag, string provider)
	{

		var content = _Content[tag.TrimStart('#').ToLowerInvariant()]
			.OrderByDescending(c => c.Timestamp)
			.FirstOrDefault(c => c.Provider == provider);

		return content?.ProviderId ?? string.Empty;

	}

	public Task<IEnumerable<(Content, ModerationAction)>> GetFilteredContentByTag(string tag, string[] providers, ModerationState[] states)
	{

		throw new NotImplementedException();

	}

	public IEnumerable<string> TagsTracked => _Content.Keys;

}
