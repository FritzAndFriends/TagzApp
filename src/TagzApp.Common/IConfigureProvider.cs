using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Common;

public interface IConfigureProvider
{

	IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration);

}