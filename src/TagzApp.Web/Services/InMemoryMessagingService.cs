using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using TagzApp.Web.Data;
using TagzApp.Web.Hubs;

namespace TagzApp.Web.Services;

public class InMemoryMessagingService : IHostedService
{

	private InMemoryContentMessaging _Service = default;

	private readonly IEnumerable<ISocialMediaProvider> _Providers;
	private readonly IHubContext<MessageHub> _HubContext;
	private readonly ILogger<InMemoryMessagingService> _Logger;

	
	public InMemoryMessagingService(
		IEnumerable<ISocialMediaProvider> providers, 
		IHubContext<MessageHub> hubContext,
		ILogger<InMemoryMessagingService> logger
	)
  {
		_Providers = providers;
		_HubContext = hubContext;
		_Logger = logger;
	}

	/// <summary>
	/// A collection of the tags and the content found for them.
	/// </summary>
	public readonly Dictionary<string, ConcurrentBag<Content>> Content = new Dictionary<string, ConcurrentBag<Content>>();
  
	#region Hosted Service Implementation

  public Task StartAsync(CancellationToken cancellationToken)
	{

		_Service = new InMemoryContentMessaging();
		_Service.StartProviders(_Providers, cancellationToken);

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

		if (!Content.ContainsKey(tag)) {
			Content.Add(tag.TrimStart('#'), new ConcurrentBag<Content>());
		}

		_Service.SubscribeToContent(new Hashtag() { Text = tag }, 
			c => {
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