using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TagzApp.Communication;
using TagzApp.Web.Data;
using TagzApp.Web.Hubs;

namespace TagzApp.Web.Services;

public class InMemoryMessagingService : BaseProviderManager, IHostedService, IMessagingService
{

	private InMemoryContentMessaging _Service = default;

	private readonly IHubContext<MessageHub> _HubContext;
	private readonly ILogger<InMemoryMessagingService> _Logger;


	public InMemoryMessagingService(
		IConfiguration configuration,
		IHubContext<MessageHub> hubContext,
		ILogger<InMemoryMessagingService> logger,
		IEnumerable<ISocialMediaProvider>? socialMediaProviders = null
	) : base(configuration, logger, socialMediaProviders)
	{
		_HubContext = hubContext;
		_Logger = logger;
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
		{   // Check hastag without #
			_Content.Add(Hashtag.ClearFormatting(tag), new ConcurrentBag<Content>());
		}

		_Service.SubscribeToContent(new Hashtag() { Text = tag },
			c =>
			{
				_Content[c.HashtagSought.TrimStart('#')].Add(c);
				_HubContext.Clients
					.Group(c.HashtagSought.TrimStart('#').ToLowerInvariant())
					.SendAsync("NewMessage", (ContentModel)c);
				//_Logger.LogInformation($"Message found for tag '{c.HashtagSought}': {c.Text}");
			});

		return Task.CompletedTask;

	}

	public IEnumerable<Content> GetExistingContentForTag(string tag)
	{

		if (!_Service._LoadedContent.ContainsKey(tag.TrimStart('#').ToLowerInvariant())) return Enumerable.Empty<Content>();

		return _Service._LoadedContent[tag.TrimStart('#').ToLowerInvariant()];

	}

	public async Task<Content> GetContentByIds(string provider, string providerId)
	{

		return _Content.First().Value
			.FirstOrDefault(c => c.Provider == provider && c.ProviderId == providerId);

	}

	public IEnumerable<string> TagsTracked => _Content.Keys;

}