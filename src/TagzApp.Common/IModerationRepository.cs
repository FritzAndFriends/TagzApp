namespace TagzApp.Common;

public interface IModerationRepository
{

	/// <summary>
	/// Mark a piece of content with a moderation action
	/// </summary>
	/// <param name="userId">User acting on the content</param>
	/// <param name="provider">The provider the content was delivered from</param>
	/// <param name="providerId">The Id on the provider for this content</param>
	/// <param name="state">The new state to apply to this content</param>
	/// <returns></returns>
	Task Moderate(string userId, string provider, string providerId, ModerationState state);

	/// <summary>
	/// Mark a piece of content with a moderation action and a reason.  No notifications are sent
	/// </summary>
	/// <param name="userId">User acting on the content</param>
	/// <param name="provider">The provider the content was delivered from</param>
	/// <param name="providerId">The Id on the provider for this content</param>
	/// <param name="state">The new state to apply to the content</param>
	/// <param name="reason">Reason for the moderation</param>
	/// <returns></returns>
	Task ModerateWithReason(string userId, string provider, string providerId, ModerationState state, string reason);

	Task<IEnumerable<Content>> GetApprovedContent(DateTimeOffset dateTimeOffset, int limit);

	Task<IEnumerable<Content>> GetAllContent(DateTimeOffset dateTimeOffset, int limit);

	Task<IEnumerable<Content>> GetRejectedContent(DateTimeOffset dateTimeOffset, int limit);

	Task<(Content Content, ModerationAction Action)> GetContentWithModeration(string provider, string providerId);


	/// <summary>
	/// Get a list of blocked users
	/// </summary>
	/// <returns>
	/// A list of blocked users
	/// </returns>
	Task<IEnumerable<BlockedUser>> GetBlockedUsers();

	Task<int> GetCurrentBlockedUserCount();


	/// <summary>
	/// Block a user
	/// </summary>
	/// <param name="userId">Id of the user to block</param>
	/// <param name="provider">Provider the user is acting on</param>
	/// <param name="userName">The moderator who is blocking the user</param>
	/// <param name="expirationDate">The date the block expires</param>
	/// <returns></returns>
	Task BlockUser(string userId, string provider, string userName, DateTimeOffset expirationDate);

	/// <summary>
	/// Unblock a user
	/// </summary>
	/// <param name="userId">Id of the user to unblock</param>
	/// <param name="provider">Provider the user is acting on</param>
	/// <returns></returns>
	Task UnblockUser(string userId, string provider);


}
