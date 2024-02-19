namespace TagzApp.ViewModels.Data;

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
public class ModerationContentModel
{

	public ModerationContentModel(
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
	DateTimeOffset? ModerationTimestamp,
	Emote[] Emotes
)
	{
		this.Provider = Provider;
		this.ProviderId = ProviderId;
		this.Type = Type;
		this.SourceUri = SourceUri;
		this.Timestamp = Timestamp;
		this.AuthorDisplayName = AuthorDisplayName;
		this.AuthorUserName = AuthorUserName;
		this.AuthorProfileUri = AuthorProfileUri;
		this.AuthorProfileImageUri = AuthorProfileImageUri;
		this.Text = Text;
		this.PreviewCard = PreviewCard;
		this.State = State;
		this.Reason = Reason;
		this.Moderator = Moderator;
		this.ModerationTimestamp = ModerationTimestamp;
		this.Emotes = Emotes;

	}

	public ContentModel Content => new(
		Provider,
		ProviderId,
		Type,
		SourceUri,
		Timestamp,
		AuthorDisplayName,
		AuthorUserName,
		AuthorProfileUri,
		AuthorProfileImageUri,
		Text,
		PreviewCard,
		Emotes
	);

	public string Provider { get; }
	public string ProviderId { get; }
	public string Type { get; }
	public string SourceUri { get; }
	public DateTimeOffset Timestamp { get; }
	public string AuthorDisplayName { get; }
	public string AuthorUserName { get; set; }
	public string AuthorProfileUri { get; set; }
	public string AuthorProfileImageUri { get; set; }
	public string Text { get; set; }
	public Card? PreviewCard { get; set; }
	public ModerationState State { get; set; }
	public string? Reason { get; set; }
	public string? Moderator { get; set; }
	public DateTimeOffset? ModerationTimestamp { get; set; }
	public Emote[] Emotes { get; set; }

	// TODO: Refactor to eliminate the direct reference to Common project
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
			action?.Timestamp,
			content.Emotes ?? new Emote[0]
		);
	}

	public static ModerationContentModel ToModerationContentModel(ContentModel content)
	{

		return new ModerationContentModel(
			content.Provider,
			content.ProviderId,
			content.Type,
			content.SourceUri,
			content.Timestamp,
			content.AuthorDisplayName,
			content.AuthorUserName,
			content.AuthorProfileUri,
			content.AuthorProfileImageUri,
			content.Text,
			content.PreviewCard,
			ModerationState.Pending,
			null,
			null,
			null,
			content.Emotes
		);

	}

}

public record NewModerator(string Email, string AvatarImageSource, string DisplayName);

public record AvailableProvider(string Id, string Name);
