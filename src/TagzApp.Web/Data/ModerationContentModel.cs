namespace TagzApp.Web.Data;

/// <summary>
/// Content to be shared with the web client
/// </summary>
/// <param name="Provider">The provider the content was identified on</param>
/// <param name="Type">The type of content identified</param>
/// <param name="SourceUri">Source URL of the content</param>
/// <param name="Timestamp">When the content was created</param>
/// <param name="AuthorDisplayName">Display name of the author of the content</param>
/// <param name="AuthorProfileUri">Profile URI of the author of the content</param>
/// <param name="AuthorProfileImageUri">Profile Image URI of the author of the content</param>
/// <param name="Text">Text of the content</param>
public record ModerationContentModel(
	string Provider,
	string ProviderId,
	string Type,
	string SourceUri,
	DateTimeOffset Timestamp,
	string AuthorDisplayName,
	string AuthorUserName,
	string AuthorProfileUri,
	string AuthorProfileImageUri,
	string Text,
	Card? PreviewCard,
	ModerationState State,
	string? Reason,
	string? Moderator,
	DateTimeOffset? ModerationTimestamp
)
{

	public static ModerationContentModel ToModerationContentModel(Content content, ModerationAction? action = null)
	{
		return new ModerationContentModel(
			content.Provider,
			content.ProviderId,
			content.Type.ToString(),
			content.SourceUri.ToString(),
			content.Timestamp,
			content.Author.DisplayName,
			content.Author.UserName,
			content.Author.ProfileUri.ToString(),
			content.Author.ProfileImageUri.ToString(),
			content.Text,
			content.PreviewCard,
			action?.State ?? ModerationState.Pending,
			action?.Reason,
			action?.Moderator,
			action?.Timestamp
		);
	}


}