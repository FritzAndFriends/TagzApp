using Microsoft.Extensions.Hosting;

namespace TagzApp.Web.Services;

public interface IMessagingService : IHostedService
{
	IEnumerable<string> TagsTracked { get; }

	Task AddHashtagToWatch(string tag);

	Task<Content?> GetContentByIds(string provider, string providerId);

	Task<IEnumerable<Content>> GetExistingContentForTag(string tag);
	// TODO: Needs checking if a non-nullable ModerationAction makes problems!
	Task<IEnumerable<(Content, ModerationAction)>> GetContentByTagForModeration(string tag);

	Task<IEnumerable<Content>> GetApprovedContentByTag(string tag);

	IEnumerable<ISocialMediaProvider> Providers { get; }

	string GetLatestProviderIdByTagAndProvider(string tag, string provider);

}
