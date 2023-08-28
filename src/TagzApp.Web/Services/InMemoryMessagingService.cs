using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TagzApp.Communication;
using TagzApp.Web.Hubs;

namespace TagzApp.Web.Services;

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
	) : base(configuration, logger, socialMediaProviders)
	{
		_Logger = logger;
		_NotifyNewMessages = notifyNewMessages;
	}

	/// <summary>
	/// A collection of the tags and the content found for them.
	/// </summary>
	private readonly Dictionary<string, ConcurrentBag<Content>> _Content = new Dictionary<string, ConcurrentBag<Content>>();

	#region Hosted Service Implementation

	public Task StartAsync(CancellationToken cancellationToken)
	{
		InitProviders();
		_Service = new InMemoryContentMessaging();
		_Service.StartProviders(Providers, cancellationToken);

		return Task.CompletedTask;
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
				_NotifyNewMessages.Notify(Hashtag.ClearFormatting(c.HashtagSought), c);
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

	public IEnumerable<string> TagsTracked => _Content.Keys;

}
