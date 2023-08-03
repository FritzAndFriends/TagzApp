﻿namespace TagzApp.Providers.Twitter.Models;


public class TwitterData
{
	public Tweet[] data { get; set; }
	public Includes includes { get; set; }
	public Meta meta { get; set; }
}

public class Includes
{
	public User[] users { get; set; }
	public Medium[] media { get; set; }
}

public class User
{
	public string id { get; set; }
	public string name { get; set; }
	public string username { get; set; }
}

public class Medium
{
	public string url { get; set; }
	public string type { get; set; }
	public int width { get; set; }
	public string media_key { get; set; }
	public int height { get; set; }
	public string alt_text { get; set; }
	public string preview_image_url { get; set; }
}

public class Meta
{
	public string newest_id { get; set; }
	public string oldest_id { get; set; }
	public int result_count { get; set; }
	public string next_token { get; set; }
}

public class Tweet
{
	public string author_id { get; set; }
	public string id { get; set; }
	public Entities entities { get; set; }
	public string[] edit_history_tweet_ids { get; set; }
	public DateTime created_at { get; set; }
	public string text { get; set; }
	public Attachments attachments { get; set; }
}

public class Entities
{
	public Mention[] mentions { get; set; }
	public Hashtag[] hashtags { get; set; }
	public Url[] urls { get; set; }
	public Annotation[] annotations { get; set; }
	public Cashtag[] cashtags { get; set; }
}

public class Mention
{
	public int start { get; set; }
	public int end { get; set; }
	public string username { get; set; }
	public string id { get; set; }
}

public class Hashtag
{
	public int start { get; set; }
	public int end { get; set; }
	public string tag { get; set; }
}

public class Url
{
	public int start { get; set; }
	public int end { get; set; }
	public string url { get; set; }
	public string expanded_url { get; set; }
	public string display_url { get; set; }
	public Image[] images { get; set; }
	public int status { get; set; }
	public string title { get; set; }
	public string description { get; set; }
	public string unwound_url { get; set; }
	public string media_key { get; set; }
}

public class Image
{
	public string url { get; set; }
	public int width { get; set; }
	public int height { get; set; }
}

public class Annotation
{
	public int start { get; set; }
	public int end { get; set; }
	public float probability { get; set; }
	public string type { get; set; }
	public string normalized_text { get; set; }
}

public class Cashtag
{
	public int start { get; set; }
	public int end { get; set; }
	public string tag { get; set; }
}

public class Attachments
{
	public string[] media_keys { get; set; }
}


