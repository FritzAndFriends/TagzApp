using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common;

namespace TagzApp.Providers.Twitter;

public class TwitterProvider : ISocialMediaProvider
{
	private readonly HttpClient _Client;

	public string Id => "TWITTER";
	public string DisplayName => "Twitter";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(15);

  public TwitterProvider(HttpClient client)
  {
		_Client = client;
  }

  public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		throw new NotImplementedException();
	}


}

public class StartTwitter : IConfigureProvider
{

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		throw new NotImplementedException();
	}

}
