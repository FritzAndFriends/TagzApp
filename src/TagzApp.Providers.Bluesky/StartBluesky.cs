using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Providers.Bluesky;

public class StartBluesky : IConfigureProvider
{
	public Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		services.AddTransient<ISocialMediaProvider, BlueskyProvider>();
		return Task.FromResult(services);
	}
}
