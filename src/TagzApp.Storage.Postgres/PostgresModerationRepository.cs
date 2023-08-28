using Microsoft.EntityFrameworkCore;
using TagzApp.Web.Services;

namespace TagzApp.Storage.Postgres;

internal class PostgresModerationRepository : IModerationRepository
{
	private readonly TagzAppContext _Context;
	private readonly INotifyNewMessages _Notifier;

	public PostgresModerationRepository(TagzAppContext context, INotifyNewMessages notifier)
	{
		_Context = context;
		_Notifier = notifier;
	}

	public async Task<IEnumerable<Content>> GetApprovedContent(DateTimeOffset dateTimeOffset, int limit)
	{

		return (await _Context.Content.AsNoTracking()
			.Include(c => c.ModerationAction)
			.Where(c => c.Timestamp >= dateTimeOffset && c.ModerationAction != null && c.ModerationAction.State == ModerationState.Approved)
			.OrderBy(c => c.Timestamp)
			.Take(limit)
			.ToArrayAsync())
			.Select(c => (Content)c).ToArray()
			;

	}

	public async Task<IEnumerable<Content>> GetPendingContent(DateTimeOffset dateTimeOffset, int limit)
	{

		return (await _Context.Content.AsNoTracking()
			.Where(c => c.Timestamp >= dateTimeOffset && c.ModerationAction == null)
			.OrderBy(c => c.Timestamp)
			.Take(limit)
			.ToArrayAsync())
			.Select(c => (Content)c).ToArray()
			;

	}

	public async Task<IEnumerable<Content>> GetRejectedContent(DateTimeOffset dateTimeOffset, int limit)
	{

		return (await _Context.Content.AsNoTracking()
			.Include(c => c.ModerationAction)
			.Where(c => c.Timestamp >= dateTimeOffset && c.ModerationAction != null && c.ModerationAction.State == ModerationState.Rejected)
			.OrderBy(c => c.Timestamp)
			.Take(limit)
			.ToArrayAsync())
			.Select(c => (Content)c).ToArray()
			;


	}

	public async Task Moderate(string userId, string provider, string providerId, ModerationState state)
	{

		var content = await _Context.Content.AsNoTracking()
			.Where(c => c.Provider == provider && c.ProviderId == providerId)
			.FirstOrDefaultAsync();
		if (content is null) throw new ArgumentOutOfRangeException("Unable to find content with ProviderId specified");

		var moderationAction = new PgModerationAction
		{
			Moderator = userId,
			Provider = provider,
			ProviderId = providerId,
			State = state,
			Timestamp = DateTimeOffset.UtcNow
		};
		_Context.ModerationActions.Add(moderationAction);
		await _Context.SaveChangesAsync();

		Action<string, Content> notify = state switch
		{
			ModerationState.Rejected => _Notifier.NotifyRejectedContent,
			ModerationState.Approved => _Notifier.NotifyApprovedContent,
			_ => _Notifier.Notify
		};

		notify(Hashtag.ClearFormatting(content.HashtagSought), (Content)content);

	}
}