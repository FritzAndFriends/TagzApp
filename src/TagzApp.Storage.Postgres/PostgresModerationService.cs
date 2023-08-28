using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace TagzApp.Storage.Postgres;

public class PostgresModerationService : IModerationSubscriber, IDisposable
{

	private CancellationTokenSource _Source = new();
	private readonly ConcurrentQueue<ActedOnContent> _Events = new();
	private bool _DisposedValue;



	public Task StartAsync(CancellationToken cancellationToken)
	{

		return Task.CompletedTask;

	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_Source.Cancel();
		return Task.CompletedTask;

	}

	private async void WatchForModeration()
	{

		var token = _Source.Token;

		while (!token.IsCancellationRequested)
		{

			if (!_Events.TryDequeue(out var evt)) {

				await Task.Delay(200);
				continue;

			}

			var actions = _Actions[evt.Tag];
			foreach (var action in actions)
			{
				action(evt.Action, evt.Content);
			}

			await Task.Delay(200);

		}

	}

	public void NotifyOfModeration(string tag, ModerationAction action, Content content)
	{
		
		_Events.Enqueue(new ActedOnContent(tag, action, content));

	}

	#region Dispose Pattern

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_Source.Cancel();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	~PostgresModerationService()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion

	private record ActedOnContent(string Tag, ModerationAction Action,  Content Content);

}