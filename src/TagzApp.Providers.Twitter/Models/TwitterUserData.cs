namespace TagzApp.Providers.Twitter.Models;


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

