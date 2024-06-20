using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using TagzApp.Communication;
using TagzApp.Security;
using TagzApp.Storage.Postgres;
using TagzApp.Storage.Postgres.SafetyModeration;
using TagzApp.Storage.Postgres.Security.Migrations;

namespace Microsoft.Extensions.DependencyInjection;

public static class AppExtensions
{

	private static Task _MigrateTask = Task.CompletedTask;

	public static IServiceCollection AddPostgresServices(this IHostApplicationBuilder builder, IConfigureTagzApp configureTagzApp, ConnectionSettings connectionSettings)
	{

		builder.AddNpgsqlDbContext<TagzAppContext>("tagzappdb");


		//services.AddScoped<IProviderConfigurationRepository, PostgresProviderConfigurationRepository>();
		builder.Services.AddSingleton<IMessagingService>(sp =>
		{
			var scope = sp.CreateScope();
			var notify = scope.ServiceProvider.GetRequiredService<INotifyNewMessages>();
			var logger = scope.ServiceProvider.GetRequiredService<ILogger<BaseProviderManager>>();
			var safetyLogger = scope.ServiceProvider.GetRequiredService<ILogger<AzureSafetyModeration>>();
			var socialMediaProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<ISocialMediaProvider>>();
			var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			return new PostgresMessagingService(sp, notify, cache, logger, safetyLogger, socialMediaProviders);
		});
		builder.Services.AddHostedService(s => s.GetRequiredService<IMessagingService>());

		builder.Services.AddScoped<IModerationRepository, PostgresModerationRepository>();
		using var builtServices = builder.Services.BuildServiceProvider();
		var ctx = builtServices.GetRequiredService<TagzAppContext>();
		ctx.Database.Migrate();

		return builder.Services;

	}

	public static IServiceCollection AddPostgresSecurityServices(this IHostApplicationBuilder builder, ConnectionSettings connectionSettings)
	{

		//builder.AddNpgsqlDbContext<TagzApp.Security.SecurityContext>("securitydb");
		builder.Services.AddNpgsql<SecurityContext>(
			builder.Configuration.GetConnectionString("securitydb"),
			options =>
			{
				options.MigrationsAssembly(typeof(SecurityContextModelSnapshot).Assembly.FullName);
			});
		builder.EnrichNpgsqlDbContext<SecurityContext>();

		var serviceLocator = builder.Services.BuildServiceProvider();
		var securityContext = serviceLocator.GetRequiredService<TagzApp.Security.SecurityContext>();

		try
		{
			securityContext.Database.Migrate();
		}
		catch (PostgresException ex)
		{
			Console.WriteLine($"Error while migrating security context to Postgres: {ex}");
		}

		return builder.Services;

	}

}
