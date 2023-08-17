namespace TagzApp.Providers.Blazot.Models;
public class Author
{
  public string UserName { get; set; }
  public string DisplayName { get; set; }
  public bool IsSubscriber { get; set; }
  public int? SubscriptionLevel { get; set; }
  public string ProfileImageUrl { get; set; }
}
