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

	public async Task<IEnumerable<Content>> GetAllContent(DateTimeOffset dateTimeOffset, int limit)
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

		var existingModAction = await _Context.ModerationActions.FirstOrDefaultAsync(m => m.Provider == provider && m.ProviderId == providerId);
		if (existingModAction is not null)
		{
			_Context.ModerationActions.Remove(existingModAction);
			content.ModerationAction = null;
			await _Context.SaveChangesAsync();
		}

		var moderationAction = new PgModerationAction
		{
			Moderator = userId,
			Provider = provider,
			ProviderId = providerId,
			State = state,
			Timestamp = DateTimeOffset.UtcNow,
			ContentId = content.Id
		};
		_Context.ModerationActions.Add(moderationAction);
		content.ModerationAction = moderationAction;
		await _Context.SaveChangesAsync();

		Action<string, Content, ModerationAction> notify = state switch
		{
			ModerationState.Approved => _Notifier.NotifyApprovedContent,
			_ => _Notifier.NotifyRejectedContent,
		};

		var outContent = (Content)content;
		notify(Hashtag.ClearFormatting(content.HashtagSought), outContent, (ModerationAction)moderationAction);
		if (state != ModerationState.Rejected) _Notifier.NotifyNewContent(Hashtag.ClearFormatting(content.HashtagSought), outContent);

	}
}
