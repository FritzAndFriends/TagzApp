namespace TagzApp.Providers.Blazot.Models;

// TODO: Check CS8618: Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class WebLink
{

	public string LinkUrl { get; set; }

	public string ImageUrl { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
