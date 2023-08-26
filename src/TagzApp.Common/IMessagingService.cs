using Microsoft.Extensions.Hosting;

namespace TagzApp.Web.Services;

public interface IMessagingService : IHostedService
{
	IEnumerable<string> TagsTracked { get; }

	Task AddHashtagToWatch(string tag);

	Task<Content?> GetContentByIds(string provider, string providerId);

	Task<IEnumerable<Content>> GetExistingContentForTag(string tag);

	IEnumerable<ISocialMediaProvider> Providers { get; }

}