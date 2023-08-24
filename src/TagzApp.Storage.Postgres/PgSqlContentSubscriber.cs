using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

public class PgSqlContentSubscriber : IContentSubscriber, IDisposable
{

	private readonly PgSqlContext _dbContext;

	private static DateTimeOffset _Since = DateTime.MinValue;

	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
	private Task? _WatchTask = null;

	private Dictionary<string,HashSet<Action<Content>>> _Actions = new();
	private bool _DisposedValue;

	public bool HasSubscribers => _Actions.Any();

	// TODO: Storage and fetch the list of tags subscribed to in the database
	public IEnumerable<string> TagsSubscribedTo => _Actions.Keys;

	public PgSqlContentSubscriber(PgSqlContext dbContext)
	{
		_dbContext = dbContext;
	}

	public void SubscribeToContentAsync(Hashtag tag, Action<Content> onNewContent)
	{

		if (_WatchTask is null) _WatchTask = Task.Run(WatchDb);

		if (!_Actions.TryGetValue(Hashtag.ClearFormatting(tag.Text), out _))
		{
			_Actions.Add(Hashtag.ClearFormatting(tag.Text), new HashSet<Action<Content>>());
		}

		_Actions[Hashtag.ClearFormatting(tag.Text)].Add(onNewContent);

	}

	private async Task WatchDb()
	{

		var token = _cancellationTokenSource.Token;

		while (!token.IsCancellationRequested)
		{

			var contents = await _dbContext.Contents
				.Where(c => c.Timestamp >= _Since)
				.OrderBy(c => c.Timestamp)
				.Take(100).ToArrayAsync();
			
			if (!contents.Any()) {
				await Task.Delay(TimeSpan.FromSeconds(1));
				continue;
			}

			_Since = contents.Max(c => c.Timestamp);

			foreach (var contentItem in contents)
			{

				if (!_Actions.TryGetValue(Hashtag.ClearFormatting(contentItem.HashtagSought), out var subscriptions)) continue;

				foreach (var subscription in subscriptions)
				{
					subscription(contentItem);
				}

			}

			await Task.Delay(TimeSpan.FromSeconds(1));

		}

	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource.Dispose();
				_WatchTask?.Wait();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~PgSqlContentSubscriber()
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
}
