using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TagzApp.Storage.Postgres;
using TagzApp.Web.Services;
using AppConfig = TagzApp.Storage.Postgres.ApplicationConfiguration;

namespace Microsoft.Extensions.DependencyInjection;

public static class AppExtensions
{

	private static Task _MigrateTask = Task.CompletedTask;

	public static IServiceCollection AddPostgresServices(this IServiceCollection services, IConfiguration configuration)
	{

		services.AddDbContext<TagzAppContext>(options =>
				{
					options.UseNpgsql(configuration.GetConnectionString("TagzApp"));
				});

		services.AddSingleton<IMessagingService, PostgresMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<IMessagingService>());

		services.AddScoped<IModerationRepository, PostgresModerationRepository>();

		var builtServices = services.BuildServiceProvider();
		var ctx = builtServices.GetRequiredService<TagzAppContext>();
		_MigrateTask = ctx.Database.MigrateAsync();

		services.AddTransient<IApplicationConfigurationRepository, AppConfig.Repository>();

		return services;

	}

	public static IConfigurationBuilder AddApplicationConfiguration(
			this IConfigurationBuilder builder)
	{
		var tempConfig = builder.Build();

		return builder.Add(new AppConfig.ConfigurationSource(tempConfig));

	}




}
