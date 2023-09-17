namespace TagzApp.Providers.Twitter.Models;
// TODO: Check all these CS8618: Non-nullable properties
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class TwitterUserData
{
	public User[] data { get; set; }

	public Error[] errors { get; set; }

	public class User
	{
		public string name { get; set; }
		public string id { get; set; }
		public string profile_image_url { get; set; }
		public string username { get; set; }
	}

	public class Error
	{
		public string value { get; set; }
		public string detail { get; set; }
		public string title { get; set; }
		public string resource_type { get; set; }
		public string parameter { get; set; }
		public string resource_id { get; set; }
		public string type { get; set; }
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
