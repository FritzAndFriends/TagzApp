using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TagzApp.Storage.Postgres;

internal class PostgresModerationRepository : IModerationRepository
{
	private readonly TagzAppContext _Context;
	private readonly INotifyNewMessages _Notifier;
	private readonly IMemoryCache _Cache;
	private const string KEY_BLOCKEDUSERS_CACHE = "blockedUsers";


	public PostgresModerationRepository(TagzAppContext context, INotifyNewMessages notifier, IMemoryCache cache)
	{
		_Context = context;
		_Notifier = notifier;
		_Cache = cache;
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

	public async Task ModerateWithReason(string userId, string provider, string providerId, ModerationState state, string reason)
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
			Reason = reason,
			State = state,
			Timestamp = DateTimeOffset.UtcNow,
			ContentId = content.Id
		};
		_Context.ModerationActions.Add(moderationAction);
		content.ModerationAction = moderationAction;
		await _Context.SaveChangesAsync();

	}

	public async Task<IEnumerable<BlockedUser>> GetBlockedUsers()
	{

		var now = DateTimeOffset.UtcNow;

		// Get the list of blocked users from the context
		return (await _Context.BlockedUsers.AsNoTracking()
			.Where(u => u.ExpirationDateTime > now)
			.ToArrayAsync())
			.Select(u => (BlockedUser)u)
			.ToArray();

	}

	public async Task<int> GetCurrentBlockedUserCount()
	{

		var now = DateTimeOffset.UtcNow;

		// Get the list of blocked users from the context
		return await _Context.BlockedUsers.AsNoTracking()
			.Where(u => u.ExpirationDateTime > now)
			.CountAsync();

	}


	public async Task BlockUser(string userId, string provider, string userName, DateTimeOffset expirationDate, BlockedUserCapabilities capabilities)
	{

		// add a new blocked user to the context
		var blockedUser = new PgBlockedUser
		{
			BlockingUser = userName,
			Provider = provider,
			UserName = userId,
			ExpirationDateTime = expirationDate,
			Capabilities = capabilities
		};
		_Context.BlockedUsers.Add(blockedUser);

		var blockedCount = await GetCurrentBlockedUserCount();
		_Notifier.NotifyNewBlockedCount(blockedCount + 1);

		await _Context.SaveChangesAsync();

		var blockedUsers = await GetBlockedUsers();
		UpdateBlockedUsersCache(blockedUsers);

	}

	public async Task UnblockUser(string userId, string provider)
	{

		// find the requested user's latest block and mark it to expire now
		var blockedUser = _Context.BlockedUsers
			.Where(u => u.Provider == provider && u.UserName == userId && u.ExpirationDateTime > DateTime.UtcNow)
			.OrderByDescending(u => u.BlockDateTime)
			.FirstOrDefault();

		if (blockedUser is null) throw new ArgumentOutOfRangeException("Unable to find blocked user");

		blockedUser.ExpirationDateTime = DateTimeOffset.UtcNow;

		var blockedCount = await GetCurrentBlockedUserCount();
		_Notifier.NotifyNewBlockedCount(blockedCount - 1);

		await _Context.SaveChangesAsync();

		var blockedUsers = await GetBlockedUsers();
		UpdateBlockedUsersCache(blockedUsers);

	}

	internal void UpdateBlockedUsersCache(IEnumerable<BlockedUser> blockedUsers)
	{
		_Cache.Set(KEY_BLOCKEDUSERS_CACHE, blockedUsers.Select(u => (u.Provider, u.UserName, u.Capabilities)).ToList());
	}

	public async Task<(Content Content, ModerationAction Action)> GetContentWithModeration(string provider, string providerId)
	{

		// Get the content item requested and include the moderation action
		var item = await _Context.Content
			.Include(c => c.ModerationAction)
			.Where(c => c.Provider == provider && c.ProviderId == providerId)
			.FirstOrDefaultAsync();

		if (item is null) throw new ArgumentException("Unable to find content with that id");
		var action = item.ModerationAction is null ? null : (ModerationAction)item.ModerationAction;

		return ((Content)item, action);

	}
}
