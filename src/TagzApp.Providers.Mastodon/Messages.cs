using System;

namespace TagzApp.Providers.Mastodon;

public class Messages
{
	public Message[] ReceivedMessages { get; set; }
}

public class Message
{
	public string id { get; set; }
	public DateTime created_at { get; set; }
	public string in_reply_to_id { get; set; }
	public string in_reply_to_account_id { get; set; }
	public bool sensitive { get; set; }
	public string spoiler_text { get; set; }
	public string visibility { get; set; }
	public string language { get; set; }
	public string uri { get; set; }
	public string url { get; set; }
	public int replies_count { get; set; }
	public int reblogs_count { get; set; }
	public int favourites_count { get; set; }
	public object edited_at { get; set; }
	public string content { get; set; }
	public object reblog { get; set; }
	public Account account { get; set; }
	public object[] media_attachments { get; set; }
	public Mention[] mentions { get; set; }
	public Tag[] tags { get; set; }
	public object[] emojis { get; set; }
	public Card card { get; set; }
	public object poll { get; set; }
}

public class Account
{
	public string id { get; set; }
	public string username { get; set; }
	public string acct { get; set; }
	public string display_name { get; set; }
	public bool locked { get; set; }
	public bool bot { get; set; }
	public bool discoverable { get; set; }
	public bool group { get; set; }
	public DateTime created_at { get; set; }
	public string note { get; set; }
	public string url { get; set; }
	public string avatar { get; set; }
	public string avatar_static { get; set; }
	public string header { get; set; }
	public string header_static { get; set; }
	public int followers_count { get; set; }
	public int following_count { get; set; }
	public int statuses_count { get; set; }
	public string last_status_at { get; set; }
	public object[] emojis { get; set; }
	public Field[] fields { get; set; }
}

public class Field
{
	public string name { get; set; }
	public string value { get; set; }
	public DateTime? verified_at { get; set; }
}

public class Card
{
	public string url { get; set; }
	public string title { get; set; }
	public string description { get; set; }
	public string language { get; set; }
	public string type { get; set; }
	public string author_name { get; set; }
	public string author_url { get; set; }
	public string provider_name { get; set; }
	public string provider_url { get; set; }
	public string html { get; set; }
	public int width { get; set; }
	public int height { get; set; }
	public string image { get; set; }
	public string embed_url { get; set; }
	public string blurhash { get; set; }
}

public class Mention
{
	public string id { get; set; }
	public string username { get; set; }
	public string url { get; set; }
	public string acct { get; set; }
}

public class Tag
{
	public string name { get; set; }
	public string url { get; set; }
}
