namespace TagzApp.Providers.Blazot.Models;

// TODO: Check CS8618: Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class Author
{

	public string UserName { get; set; }

	public string DisplayName { get; set; }
	public bool IsSubscriber { get; set; }
	public int? SubscriptionLevel { get; set; }
	public string ProfileImageUrl { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
