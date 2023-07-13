using System.Collections;
using System.Collections.Concurrent;

namespace TagzApp.Common;

public class InMemoryContentMessaging : IContentPublisher, IContentSubscriber, IDisposable
{

	internal readonly Dictionary<string, ConcurrentQueue<Content>> Queue = new();
	private readonly Dictionary<string, ConcurrentBag<Action<Content>>> _Actions = new();

	private readonly Task _QueueWatcher = Task.CompletedTask;
	private CancellationTokenSource _CancellationTokenSource;

	private bool _DisposedValue;

  public InMemoryContentMessaging()
  {
		_CancellationTokenSource = new CancellationTokenSource();
		_QueueWatcher = Task.Run(WatchQueue);
  }

	private async Task WatchQueue()
	{

		var token = _CancellationTokenSource.Token;

		while (!token.IsCancellationRequested)
		{

			foreach (var queue in Queue) {

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

		if (!_Actions.ContainsKey(tag.Text)) {
			_Actions.Add(tag.Text, new ConcurrentBag<Action<Content>>());
		}

		_Actions[tag.Text].Add(onNewContent);

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