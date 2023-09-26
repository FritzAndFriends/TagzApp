using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TagzApp.Communication;
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

		services.AddScoped<IProviderConfigurationRepository, PostgresProviderConfigurationRepository>();
		services.AddSingleton<IMessagingService>(sp =>
		{
			var scope = sp.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<IProviderConfigurationRepository>();
			var notify = scope.ServiceProvider.GetRequiredService<INotifyNewMessages>();
			var logger = scope.ServiceProvider.GetRequiredService<ILogger<BaseProviderManager>>();
			var socialMediaProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<ISocialMediaProvider>>();
			var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
			return new PostgresMessagingService(sp, notify, config, logger, socialMediaProviders, repo);
		});
		services.AddHostedService(s => s.GetRequiredService<IMessagingService>());

		services.AddScoped<IModerationRepository, PostgresModerationRepository>();
		using var builtServices = services.BuildServiceProvider();
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
