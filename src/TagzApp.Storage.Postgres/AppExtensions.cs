using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using TagzApp.Communication;
using TagzApp.Storage.Postgres;
using TagzApp.Storage.Postgres.SafetyModeration;

namespace Microsoft.Extensions.DependencyInjection;

public static class AppExtensions
{

	private static Task _MigrateTask = Task.CompletedTask;

	public static IServiceCollection AddPostgresServices(this IServiceCollection services, IConfigureTagzApp configureTagzApp, ConnectionSettings connectionSettings)
	{

		services.AddDbContext<TagzAppContext>(options =>
				{
					options.UseNpgsql(connectionSettings.ContentConnectionString);
				});

		//services.AddScoped<IProviderConfigurationRepository, PostgresProviderConfigurationRepository>();
		services.AddSingleton<IMessagingService>(sp =>
		{
			var scope = sp.CreateScope();
			var notify = scope.ServiceProvider.GetRequiredService<INotifyNewMessages>();
			var logger = scope.ServiceProvider.GetRequiredService<ILogger<BaseProviderManager>>();
			var safetyLogger = scope.ServiceProvider.GetRequiredService<ILogger<AzureSafetyModeration>>();
			var socialMediaProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<ISocialMediaProvider>>();
			var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			return new PostgresMessagingService(sp, notify, cache, logger, safetyLogger, socialMediaProviders);
		});
		services.AddHostedService(s => s.GetRequiredService<IMessagingService>());

		services.AddScoped<IModerationRepository, PostgresModerationRepository>();
		using var builtServices = services.BuildServiceProvider();
		var ctx = builtServices.GetRequiredService<TagzAppContext>();
		_MigrateTask = ctx.Database.MigrateAsync();

		return services;

	}

	public static IServiceCollection AddPostgresSecurityServices(this IServiceCollection services, ConnectionSettings connectionSettings)
	{

		services.AddDbContext<TagzApp.Security.SecurityContext>(options =>
		{
			options.UseNpgsql(connectionSettings.SecurityConnectionString,
			pg => pg.MigrationsAssembly(typeof(TagzApp.Storage.Postgres.Security.Migrations.SecurityContextModelSnapshot).Assembly.FullName));
		}); //, ServiceLifetime.Transient);

		var serviceLocator = services.BuildServiceProvider();
		var securityContext = serviceLocator.GetRequiredService<TagzApp.Security.SecurityContext>();

		try
		{
			securityContext.Database.Migrate();
		}
		catch (PostgresException ex)
		{
			Console.WriteLine($"Error while migrating security context to Postgres: {ex}");
		}

		return services;

	}

}
