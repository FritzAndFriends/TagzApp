namespace TagzApp.Providers.Twitter;

public class TwitterConfiguration
{

	/// <summary>
	/// Twitter issued API Key for the service
	/// </summary>
  public string ApiKey { get; set; }

	/// <summary>
	/// Twitter issued API Secret Key for the service
	/// </summary>
  public string ApiSecretKey { get; set; }

	/// <summary>
	/// Twitter issued Bearer Token for the service
	/// </summary>
	public string BearerToken { get; set; }

	/// <summary>
	/// Access token for Twitter
	/// </summary>
  public string AccessToken { get; set; }

	/// <summary>
	/// Access token secret for Twitter
	/// </summary>
  public string AccessTokenSecret { get; set; }

}
