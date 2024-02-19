using Microsoft.Extensions.Hosting;

namespace TagzApp.Common;

public interface IMessagingService : IHostedService
{
	IEnumerable<string> TagsTracked { get; }

	Task AddHashtagToWatch(string tag);

	Task<Content?> GetContentByIds(string provider, string providerId);

	Task<IEnumerable<Content>> GetExistingContentForTag(string tag);
	// TODO: Needs checking if a non-nullable ModerationAction makes problems!
	Task<IEnumerable<(Content, ModerationAction)>> GetContentByTagForModeration(string tag);

	Task<IEnumerable<(Content, ModerationAction)>> GetFilteredContentByTag(string tag, string[] providers, ModerationState[] states);

	Task<IEnumerable<Content>> GetApprovedContentByTag(string tag);

	IEnumerable<ISocialMediaProvider> Providers { get; }

	string GetLatestProviderIdByTagAndProvider(string tag, string provider);

}
