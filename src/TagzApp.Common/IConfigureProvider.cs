using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Common;

/// <summary>
/// An interface to configure the services for a Social Media Provider
/// </summary>
public interface IConfigureProvider
{
	/// <summary>
	/// Register all of the services with dependency injection for the social media provider
	/// </summary>
	/// <param name="services">The dependency injection services</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Service Collection</returns>
	Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default);

}
