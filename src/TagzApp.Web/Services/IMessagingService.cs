namespace TagzApp.Web.Services;

public interface IMessagingService : IHostedService
{
	IEnumerable<string> TagsTracked { get; }

	Task AddHashtagToWatch(string tag);

	Task<Content> GetContentByIds(string provider, string providerId);

	IEnumerable<Content> GetExistingContentForTag(string tag);

	IEnumerable<ISocialMediaProvider> Providers { get; }

}