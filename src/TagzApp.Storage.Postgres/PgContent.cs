using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TagzApp.Storage.Postgres;

internal class PgContent
{

	public long Id { get; set; }

	[MaxLength(20)]
	public required string Provider { get; set; }

	[MaxLength(200)]
	public required string ProviderId { get; set; }

	[MaxLength(50)]
	public string HashtagSought { get; set; } = string.Empty;

	public ContentType Type { get; set; } = ContentType.Message;

	[MaxLength(1000)]
	public required string SourceUri { get; set; }

	public DateTimeOffset Timestamp { get; set; }

	public required string Author { get; set; }

	public string Text { get; set; } = string.Empty;

	public string? PreviewCard { get; set; }

	public string? Emotes { get; set; }

	public PgModerationAction? ModerationAction { get; internal set; }

	public static explicit operator Content(PgContent thisContent)
	{

		var author = JsonSerializer.Deserialize<Creator>(thisContent.Author);
		var card = string.IsNullOrEmpty(thisContent.PreviewCard) ? null : JsonSerializer.Deserialize<Card>(thisContent.PreviewCard);

		return new Content
		{

			Id = thisContent.Id,
			// TODO: Check if author might be null at any	point because the compiler thinks it might be (Creator? is returned!)
			Author = author!,
			Provider = thisContent.Provider,
			ProviderId = thisContent.ProviderId,
			HashtagSought = thisContent.HashtagSought,
			Type = thisContent.Type,
			SourceUri = new Uri(thisContent.SourceUri),
			Timestamp = thisContent.Timestamp,
			Text = thisContent.Text,
			PreviewCard = card,
			Emotes = string.IsNullOrEmpty(thisContent.Emotes) ? null : JsonSerializer.Deserialize<Emote[]>(thisContent.Emotes)

		};

	}

	public static explicit operator PgContent(Content content)
	{

		return new PgContent
		{
			Author = JsonSerializer.Serialize(content.Author),
			Provider = content.Provider,
			ProviderId = content.ProviderId,
			HashtagSought = content.HashtagSought,
			Type = content.Type,
			SourceUri = content.SourceUri.ToString(),
			Timestamp = content.Timestamp,
			Text = content.Text,
			PreviewCard = content.PreviewCard == null ? null : JsonSerializer.Serialize(content.PreviewCard),
			Emotes = content.Emotes == null ? null : JsonSerializer.Serialize(content.Emotes),
			Id = content.Id

		};

	}

}
