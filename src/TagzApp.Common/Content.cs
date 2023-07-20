namespace TagzApp.Common;

public class Content
{

	/// <summary>
	/// Unique identifier for the content
	/// </summary>
	public long Id { get; internal set; }

	/// <summary>
	/// Provider that the content came from
	/// </summary>
	public required string ProviderId { get; set; }

	public string HashtagSought { get; set; } = string.Empty;

	/// <summary>
	/// Type of the content identifier
	/// </summary>
	public ContentType Type { get; set; } = ContentType.Message;

	/// <summary>
	/// Original source of this content
	/// </summary>
	public required Uri SourceUri { get; set; }

	/// <summary>
	/// Date and time when the content was created
	/// </summary>
  public DateTimeOffset Timestamp { get; set; }

	/// <summary>
	/// The account who created the content
	/// </summary>
  public required Creator Author { get; set; }

	/// <summary>
	/// Text associated with this content
	/// </summary>
  public string Text { get; set; } = string.Empty;

}
