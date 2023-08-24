using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using TagzApp.Web.Data;
using TagzApp.Web.Hubs;
using TagzApp.Web.Services.Base;

namespace TagzApp.Web.Services;

public class InMemoryMessagingService : BaseProviderManager, IHostedService
{
	private InMemoryContentMessaging? _Service = default;

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
	public readonly Dictionary<string, ConcurrentBag<Content>> Content = new();

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
		if (!Content.ContainsKey(Hashtag.ClearFormatting(tag)))
		{
			// Check hastag without #
			Content.Add(Hashtag.ClearFormatting(tag), new ConcurrentBag<Content>());
		}

		_Service.SubscribeToContent(new Hashtag() { Text = tag },
			c =>
			{
				Content[c.HashtagSought.TrimStart('#')].Add(c);
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
}