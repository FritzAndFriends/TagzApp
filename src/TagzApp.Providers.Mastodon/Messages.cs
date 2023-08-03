using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TagzApp.Providers.Mastodon;

public class Messages
{
	public Message[]? ReceivedMessages { get; set; }
}

[DebuggerDisplay("{account.display_name}: {content}")]
public class Message
{
	public string id { get; set; } = string.Empty;
	public DateTime created_at { get; set; }
	public string in_reply_to_id { get; set; } = string.Empty;
	public string in_reply_to_account_id { get; set; } = string.Empty;
	public bool sensitive { get; set; }
	public string spoiler_text { get; set; } = string.Empty;
	public string visibility { get; set; } = string.Empty;
	public string language { get; set; } = string.Empty;
	public string uri { get; set; } = string.Empty;
	public string url { get; set; } = string.Empty;
	public int replies_count { get; set; }
	public int reblogs_count { get; set; }
	public int favourites_count { get; set; }
	public object? edited_at { get; set; }
	public string content { get; set; } = string.Empty;
	public object? reblog { get; set; }
	public Account? account { get; set; }
	public object[]? media_attachments { get; set; }
	public Mention[]? mentions { get; set; }
	public Tag[]? tags { get; set; }
	public object[]? emojis { get; set; }
	public Card? card { get; set; }
	public object? poll { get; set; }

	public static MediaAttachment GetMediaAttachment(string mediaJson)
	{

		var rawJson = mediaJson.Substring(mediaJson.IndexOf('{'));
		rawJson = rawJson.TrimEnd('"');

		return JsonSerializer.Deserialize<MediaAttachment>(rawJson);

	}

}

public class Account
{
	public string id { get; set; } = string.Empty;
	public string username { get; set; } = string.Empty;
	public string acct { get; set; } = string.Empty;
	public string display_name { get; set; } = string.Empty;
	public bool locked { get; set; }
	public bool bot { get; set; }
	public object discoverable { get; set; }
	public bool group { get; set; }
	public DateTime created_at { get; set; }
	public string note { get; set; } = string.Empty;
	public string url { get; set; } = string.Empty;
	public string avatar { get; set; } = string.Empty;
	public string avatar_static { get; set; } = string.Empty;
	public string header { get; set; } = string.Empty;
	public string header_static { get; set; } = string.Empty;
	public int followers_count { get; set; }
	public int following_count { get; set; }
	public int statuses_count { get; set; }
	public string last_status_at { get; set; } = string.Empty;
	public object[]? emojis { get; set; }
	public Field[]? fields { get; set; }
}

public class Field
{
	public string name { get; set; } = string.Empty;
	public string value { get; set; } = string.Empty;
	public DateTime? verified_at { get; set; }
}

public class Card
{
	public string url { get; set; } = string.Empty;
	public string title { get; set; } = string.Empty;
	public string description { get; set; } = string.Empty;
	public string language { get; set; } = string.Empty;
	public string type { get; set; } = string.Empty;
	public string author_name { get; set; } = string.Empty;
	public string author_url { get; set; } = string.Empty;
	public string provider_name { get; set; } = string.Empty;
	public string provider_url { get; set; } = string.Empty;
	public string html { get; set; } = string.Empty;
	public int width { get; set; }
	public int height { get; set; }
	public string image { get; set; } = string.Empty;
	public string embed_url { get; set; } = string.Empty;
	public string blurhash { get; set; } = string.Empty;

	public static explicit operator Common.Card?(Card card)
	{

		if (card?.image is null) return null;

		return new Common.Card
		{
			ImageUri = new Uri(card.image),
			AltText = card.title,
			Height = card.height,
			Width = card.width
		};

	}

}

public class Mention
{
	public string id { get; set; } = string.Empty;
	public string username { get; set; } = string.Empty;
	public string url { get; set; } = string.Empty;
	public string acct { get; set; } = string.Empty;
}

public class Tag
{
	public string name { get; set; } = string.Empty;
	public string url { get; set; } = string.Empty;
}



public class MediaAttachment
{
	public string id { get; set; }
	public string type { get; set; }
	public string url { get; set; }
	public string preview_url { get; set; }
	public object remote_url { get; set; }
	public object preview_remote_url { get; set; }
	public object text_url { get; set; }
	public Meta meta { get; set; }
	public object description { get; set; }
	public string blurhash { get; set; }

	public static implicit operator Common.Card(MediaAttachment mediaAttachment)
	{

		if (mediaAttachment is null) return null;

		return new Common.Card
		{
			ImageUri = new Uri(mediaAttachment.url),
			AltText = mediaAttachment.description?.ToString(),
			Height = mediaAttachment.meta?.original?.height ?? 0,
			Width = mediaAttachment.meta?.original?.width ?? 0
		};

	}

}

public class Meta
{
	public Original original { get; set; }
	public Small small { get; set; }
}

public class Original
{
	public int width { get; set; }
	public int height { get; set; }
	public string size { get; set; }
	public float aspect { get; set; }
}

public class Small
{
	public int width { get; set; }
	public int height { get; set; }
	public string size { get; set; }
	public float aspect { get; set; }
}
