namespace TagzApp.Web.Services;

public interface IMessagingService
{
	IEnumerable<string> TagsTracked { get; }

	Task AddHashtagToWatch(string tag);

	Task<Content> GetContentByIds(string provider, string providerId);

	IEnumerable<Content> GetExistingContentForTag(string tag);

	Task StartAsync(CancellationToken cancellationToken);
	Task StopAsync(CancellationToken cancellationToken);

}