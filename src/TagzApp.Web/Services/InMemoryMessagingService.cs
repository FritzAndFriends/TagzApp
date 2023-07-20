﻿using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace TagzApp.Web.Services;

public class InMemoryMessagingService : IHostedService
{

	private InMemoryContentMessaging _Service = default;

	public readonly Dictionary<string, ConcurrentBag<Content>> Content = new Dictionary<string, ConcurrentBag<Content>>();
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
				_HubContext.Clients.All.SendAsync("NewMessage", c);
				//_Logger.LogInformation($"Message found for tag '{c.HashtagSought}': {c.Text}");
			});

		return Task.CompletedTask;

	}

}
