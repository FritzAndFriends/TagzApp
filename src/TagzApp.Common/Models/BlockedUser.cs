namespace TagzApp.Common.Models;

/// <summary>
/// A user that is blocked defined by the service they're writing content on
/// </summary>
public class BlockedUser
{

	/// <summary>
	/// The name of the provider this user is operating on
	/// </summary>
	public string Provider { get; set; }

	/// <summary>
	/// The id of the user on that provider
	/// </summary>
	public string UserName { get; set; }

	/// <summary>
	/// Who was the moderator that blocked this user 
	/// </summary>
	public string BlockingUser { get; set; }

	/// <summary>
	/// When was this user blocked
	/// </summary>
	public DateTimeOffset BlockedDate { get; set; }

	/// <summary>
	/// When will this user be unblocked
	/// </summary>
	public DateTimeOffset? ExpirationDate { get; set; }

}
