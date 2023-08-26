using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TagzApp.Storage.Postgres;
using TagzApp.Web.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class AppExtensions
{

	public static IServiceCollection AddPostgresServices(this IServiceCollection services, IConfiguration configuration)
	{

		services.AddDbContext<TagzAppContext>(options =>
				{
			options.UseNpgsql(configuration.GetConnectionString("TagzApp"));
		});

		services.AddSingleton<IMessagingService, PostgresMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<IMessagingService>());

		return services;

	}
	

}
