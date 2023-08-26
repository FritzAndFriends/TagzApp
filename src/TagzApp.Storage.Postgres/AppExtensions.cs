using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Storage.Postgres;

public static class AppExtensions
{

	public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
	{

		services.AddDbContext<TagzAppContext>(options =>
				{
			options.UseNpgsql(configuration.GetConnectionString("TagzApp"));
		});

		return services;

	}
	

}
