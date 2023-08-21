namespace TagzApp.Providers.Twitter.Models;

public class TwitterData
{
	public required Tweet[] data { get; set; }
	public required Includes includes { get; set; }
	public required Meta meta { get; set; }
}

public class Includes
{
	public required Medium[] media { get; set; }
	public required User[] users { get; set; }
}

public class Medium
{
	public required string media_key { get; set; }
	public string? preview_image_url { get; set; }
	public required string type { get; set; }
	public string? alt_text { get; set; }
	public int? height { get; set; }
	public int? width { get; set; }
}

public class User
{
	public required string profile_image_url { get; set; }
	public required string username { get; set; }
	public required string name { get; set; }
	public required string id { get; set; }
}

public class Meta
{
	public required string newest_id { get; set; }
	public required string oldest_id { get; set; }
	public int result_count { get; set; }
	public required string next_token { get; set; }
}

public class Tweet
{
	public required string author_id { get; set; }
	public DateTime created_at { get; set; }
	public required string text { get; set; }
	public required Entities entities { get; set; }
	public required string[] edit_history_tweet_ids { get; set; }
	public required Attachments attachments { get; set; }
	public required string id { get; set; }
}

public class Entities
{
	public required Url[] urls { get; set; }
	public required Hashtag[] hashtags { get; set; }
	public required Annotation[] annotations { get; set; }
	public required Mention[] mentions { get; set; }
	public required Cashtag[] cashtags { get; set; }
}

public class Url
{
	public int start { get; set; }
	public int end { get; set; }
	public required string url { get; set; }
	public required string expanded_url { get; set; }
	public required string display_url { get; set; }
	public required string media_key { get; set; }
	public required Image[] images { get; set; }
	public int status { get; set; }
	public required string title { get; set; }
	public required string description { get; set; }
	public required string unwound_url { get; set; }
}

public class Image
{
	public required string url { get; set; }
	public int width { get; set; }
	public int height { get; set; }
}

public class Hashtag
{
	public int start { get; set; }
	public int end { get; set; }
	public required string tag { get; set; }
}

public class Annotation
{
	public int start { get; set; }
	public int end { get; set; }
	public float probability { get; set; }
	public required string type { get; set; }
	public required string normalized_text { get; set; }
}

public class Mention
{
	public int start { get; set; }
	public int end { get; set; }
	public required string username { get; set; }
	public required string id { get; set; }
}

public class Cashtag
{
	public int start { get; set; }
	public int end { get; set; }
	public required string tag { get; set; }
}

public class Attachments
{
	public required string[] media_keys { get; set; }
}