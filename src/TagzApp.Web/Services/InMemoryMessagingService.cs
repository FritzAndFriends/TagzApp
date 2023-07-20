using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace TagzApp.Web.Services;

public class InMemoryMessagingService : IHostedService
{

	private InMemoryContentMessaging _Service = default;

	public readonly Dictionary<string, ConcurrentBag<Content>> Content = new Dictionary<string, ConcurrentBag<Content>>();

	#region Hosted Service Implementation

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_Service = new InMemoryContentMessaging();
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
			Content.Add(tag, new ConcurrentBag<Content>());
		}

		_Service.SubscribeToContent(new Hashtag() { Text = tag }, 
			c => Content[c.HashtagSought].Add(c));

		return Task.CompletedTask;

	}

}
