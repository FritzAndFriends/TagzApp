namespace TagzApp.Common.Models;

public class Content
{
	/// <summary>
	///   Unique identifier for the content
	/// </summary>
	public long Id { get; set; }

	/// <summary>
	///   Provider that the content came from
	/// </summary>
	public required string Provider { get; set; }

	public required string ProviderId { get; set; }

	public string HashtagSought { get; set; } = string.Empty;

	/// <summary>
	///   Type of the content identifier
	/// </summary>
	public ContentType Type { get; set; } = ContentType.Message;

	/// <summary>
	///   Original source of this content
	/// </summary>
	public required Uri SourceUri { get; set; }

	/// <summary>
	///   Date and time when the content was created
	/// </summary>
	public DateTimeOffset Timestamp { get; set; }

	/// <summary>
	///   The account who created the content
	/// </summary>
	public required Creator Author { get; set; }

	/// <summary>
	///   Text associated with this content
	/// </summary>
	public string Text { get; set; } = string.Empty;

	public Card? PreviewCard { get; set; }

	public Emote[]? Emotes { get; set; }
}

/// <summary>
///   Definition of an emote to replace in a chat message with an image
/// </summary>
/// <param name="Pos">Position of the text to replace</param>
/// <param name="Length">Length of the text to replace</param>
/// <param name="ImageUrl">Image URL to replace with</param>
public record Emote(int Pos, int Length, string ImageUrl);
