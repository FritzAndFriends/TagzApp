using System.Collections;
using System.Collections.Concurrent;
using System.Web;

namespace TagzApp.Common;

public class InMemoryContentMessaging : IContentPublisher, IContentSubscriber, IDisposable
{

	internal readonly Dictionary<string, ConcurrentQueue<Content>> Queue = new();
	private readonly Dictionary<string, ConcurrentBag<Action<Content>>> _Actions = new();

	public readonly ConcurrentDictionary<string, ConcurrentBag<Content>> _LoadedContent = new();

	private readonly Task _QueueWatcher = Task.CompletedTask;
	private CancellationTokenSource _CancellationTokenSource;

	private List<Task> _ProviderTasks = new List<Task>();

	private bool _DisposedValue;

	public InMemoryContentMessaging()
	{
		_CancellationTokenSource = new CancellationTokenSource();
		_QueueWatcher = Task.Run(DispatchFromQueue);
	}

	private async Task DispatchFromQueue()
	{

		var token = _CancellationTokenSource.Token;

		while (!token.IsCancellationRequested)
		{

			foreach (var queue in Queue)
			{

				if (queue.Value.TryDequeue(out var content))
				{

					if (_Actions.ContainsKey(queue.Key))
					{
						foreach (var action in _Actions[queue.Key])
						{
							action(content);
						}
					}

				}


			}

			await Task.Delay(100);

	}

	}

	public Task PublishContentAsync(Hashtag tag, Content newContent)
	{

		if (!Queue.ContainsKey(tag.Text))
		{
			Queue.Add(tag.Text, new ConcurrentQueue<Content>());
		}

		Queue[tag.Text].Enqueue(newContent);

		return Task.CompletedTask;

	}

	public void SubscribeToContent(Hashtag tag, Action<Content> onNewContent)
	{

		if (!_Actions.ContainsKey(tag.Text))
		{
			_Actions.Add(tag.Text, new ConcurrentBag<Action<Content>>());
		}

		_Actions[tag.Text].Add(onNewContent);

	}

	public void StartProviders(IEnumerable<ISocialMediaProvider> providers, CancellationToken cancellationToken)
	{

		_ProviderTasks.Clear();
		foreach (var providerItem in providers)
		{

			_ProviderTasks.Add(Task.Factory.StartNew(async (object state) =>
			{

				var provider = (ISocialMediaProvider)state;

				var lastQueryTime = DateTimeOffset.UtcNow.AddHours(-1);

				while (!cancellationToken.IsCancellationRequested)
				{

					if (!_Actions.Any()) {
						await Task.Delay(TimeSpan.FromSeconds(1));
						continue;
					}

					foreach (var tag in _Actions.Keys.Distinct<string>())
					{

						var formattedTag = tag.TrimStart('#').ToLowerInvariant();

            if (!_LoadedContent.ContainsKey(formattedTag))
            {
							_LoadedContent.TryAdd(formattedTag, new());
            }

						var searchTime = lastQueryTime;
						lastQueryTime = DateTime.UtcNow;
						Hashtag thisTag = new Hashtag() { Text = tag };
						var contentIdentified = await provider.GetContentForHashtag(thisTag, searchTime);

						// de-dupe with in-memory collection
						if (contentIdentified != null && contentIdentified.Any())
						{
							contentIdentified = contentIdentified
								.DistinctBy(c => new { c.Provider, c.ProviderId })
								.ExceptBy(_LoadedContent[tag.TrimStart('#').ToLowerInvariant()].Select(c => c.ProviderId).ToArray(), c => c.ProviderId)
								.ToArray();

							foreach (var item in contentIdentified.OrderBy<Content, DateTimeOffset>(c => c.Timestamp))
							{
								_LoadedContent[tag.TrimStart('#').ToLowerInvariant()].Add(item);
								await PublishContentAsync(thisTag, item);
							}

						}

						await Task.Delay(provider.NewContentRetrievalFrequency);

					}

				}

			}, providerItem));

		}

	}


	#region Dispose Pattern 

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_CancellationTokenSource.Cancel();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~InMemoryContentMessaging()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion

}