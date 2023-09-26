using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web;

public static class ServicesExtensions
{

	public static IServiceCollection AddTagzAppHostedServices(this IServiceCollection services, IConfigurationRoot configuration)
	{

		services.AddSingleton<INotifyNewMessages, SignalRNotifier>();

		if (!string.IsNullOrEmpty(configuration.GetConnectionString("TagzApp")))
		{
			services.AddPostgresServices(configuration);
		}
		else
		{
			services.AddSingleton<IProviderConfigurationRepository, InMemoryProviderConfigurationRepository>();
			services.AddSingleton<IMessagingService, InMemoryMessagingService>();
			services.AddHostedService(s => s.GetRequiredService<IMessagingService>());
		}

		return services;
	}

	/// <summary>
	/// A collection of externally configured providers
	/// </summary>
	public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name,
		IConfiguration configuration,
		Action<IConfiguration> action)
	{
		var section = configuration.GetSection($"Authentication:{name}");
		if (section is not null) action(section);
		return builder;
	}

	public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name,
		IConfiguration configuration,
		Action<Action<Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions>> action)
	{
		return builder.AddExternalProvider(name, configuration, (section) =>
		{
			var clientID = section["ClientID"];
			var clientSecret = section["ClientSecret"];
			if (!string.IsNullOrEmpty(clientID) && !string.IsNullOrEmpty(clientSecret))
			{
				action(options =>
				{
					options.ClientId = clientID;
					options.ClientSecret = clientSecret;
				});
			}
		});
	}

	public static AuthenticationBuilder AddExternalProviders(this AuthenticationBuilder builder,
		IConfiguration configuration)
	{
		builder.AddExternalProvider("Microsoft", configuration, options => builder.AddMicrosoftAccount(options));
		builder.AddExternalProvider("GitHub", configuration, options => builder.AddGitHub(options));
		builder.AddExternalProvider("LinkedIn", configuration, options => builder.AddLinkedIn(options));

		return builder;
	}

	public static async Task InitializeSecurity(this IServiceProvider services)
	{
		using var scope = services.CreateScope();

		// create database if not exists
		var dbContext = scope.ServiceProvider.GetRequiredService<SecurityContext>();
		if (dbContext.Database.ProviderName!.Equals("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.InvariantCultureIgnoreCase)) await dbContext.Database.EnsureCreatedAsync();

		var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
		if (!(await roleManager.RoleExistsAsync(Security.Role.Admin)))
		{
			await roleManager.CreateAsync(new IdentityRole(Security.Role.Admin));
		}

		if (!(await roleManager.RoleExistsAsync(Security.Role.Moderator)))
		{
			await roleManager.CreateAsync(new IdentityRole(Security.Role.Moderator));
		}
	}

	public static void AddSecurityContext(this IServiceCollection services, IConfiguration configuration)
	{

		if (!string.IsNullOrEmpty(configuration.GetConnectionString("TagzAppSecurity")))
		{

			services.AddDbContext<SecurityContext>(options =>
			{
				options.UseNpgsql(configuration.GetConnectionString("TagzAppSecurity"),
				pg => pg.MigrationsAssembly("TagzApp.Storage.Postgres.Security"));
			});

		}
		else if (!string.IsNullOrEmpty(configuration.GetConnectionString("SecurityContextConnection")))
		{

			services.AddDbContext<SecurityContext>(options =>
			{
				options.UseSqlite(configuration.GetConnectionString("SecurityContextConnection"));
			});

		}

	}

}
