using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

public class PgModerationSubscriber : IModerationSubscriber
{
	private readonly PgSqlContext _dbContext;

	private static DateTimeOffset _Since = DateTime.MinValue;

	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
	private Task? _WatchTask = null;

	private Dictionary<string, HashSet<Action<ModeratedContent>>> _Actions = new();
	private bool _DisposedValue;


	public PgModerationSubscriber(PgSqlContext context)
	{
		_dbContext = context;
	}

	public void SubscribeToModerationAsync(Hashtag tag, Action<ModeratedContent> onNewContent)
	{
		if (_WatchTask is null) _WatchTask = Task.Run(WatchDb);

		if (!_Actions.TryGetValue(Hashtag.ClearFormatting(tag.Text), out _))
		{
			_Actions.Add(Hashtag.ClearFormatting(tag.Text), new());
		}

		_Actions[Hashtag.ClearFormatting(tag.Text)].Add(onNewContent);
	}

	private async Task WatchDb()
	{
		var token = _cancellationTokenSource.Token;

		while (!token.IsCancellationRequested)
		{

			var modActions = await _dbContext.ModerationActions
				.Where(c => c.Timestamp >= _Since)
				.Include(c => c.Content)
				.OrderBy(c => c.Timestamp)
				.Take(100).ToArrayAsync();

			if (!modActions.Any())
			{
				await Task.Delay(TimeSpan.FromSeconds(1));
				continue;
			}

			_Since = modActions.Max(c => c.Timestamp);

			foreach (var modAction in modActions)
			{

				if (!_Actions.TryGetValue(Hashtag.ClearFormatting(modAction.Content.HashtagSought), out var subscriptions)) continue;

				var outContent = (ModeratedContent)modAction.Content;
				outContent.State = modAction.State;
				outContent.UserId = modAction.UserId;
				outContent.Timestamp = modAction.Timestamp;

				foreach (var subscription in subscriptions)
				{
					subscription(outContent);
				}

			}

			await Task.Delay(TimeSpan.FromSeconds(1));

		}

	}
}
