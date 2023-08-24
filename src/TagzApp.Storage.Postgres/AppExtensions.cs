using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
using TagzApp.Storage.Postgres;

namespace Microsoft.Extensions.DependencyInjection;

public static class AppExtensions
{

	public static IServiceCollection AddPostgresStorage(this IServiceCollection services, IConfiguration configuration)
	{

		services.AddDbContext<PgSqlContext>(options =>
				{
					var connectionString = configuration.GetConnectionString("pgStorage");
					options.UseNpgsql(connectionString);
				});

		services.AddSingleton<PgSqlContentPublisher>();
		services.AddSingleton<PgSqlContentSubscriber>();
		services.AddSingleton<PgModerationPublisher>();
		services.AddSingleton<PgModerationSubscriber>();

		return services;

	}


}

