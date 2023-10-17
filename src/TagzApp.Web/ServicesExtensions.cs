using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TagzApp.Providers.YouTubeChat;
using TagzApp.Storage.Postgres;
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
			services.AddScoped<IProviderConfigurationRepository, PostgresProviderConfigurationRepository>();
			services.AddPostgresServices(configuration);
		}
		else
		{
			services.AddSingleton<IMessagingService, InMemoryMessagingService>();
			services.AddHostedService(s => s.GetRequiredService<IMessagingService>());
			services.AddSingleton<IProviderConfigurationRepository, InMemoryProviderConfigurationRepository>();
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

		if (!string.IsNullOrEmpty(configuration["Authentication:Google:ClientId"]))
		{
			builder.AddGoogle(options =>
			{
				options.ClientId = configuration[YouTubeChatConfiguration.Key_Google_ClientId];
				options.ClientSecret = configuration[YouTubeChatConfiguration.Key_Google_ClientSecret];
				options.SaveTokens = true;
				options.AccessType = "offline";  // Allow a refresh token to be delivered
				options.Scope.Add(YouTubeChatConfiguration.Scope_YouTube);
				options.Events.OnTicketReceived = ctx =>
				{
					var tokens = ctx.Properties.GetTokens().ToList();
					tokens.Add(new AuthenticationToken
					{
						Name = "Ticket Created",
						Value = DateTime.UtcNow.ToString()
					});
					tokens.Add(new AuthenticationToken
					{
						Name = "Email",
						Value = ctx.Principal.Claims.First(c => c.Type == ClaimTypes.Email).Value
					});
					ctx.Properties.StoreTokens(tokens);
					return Task.CompletedTask;
				};
			});
		}

		return builder;
	}

	public static async Task InitializeSecurity(this IServiceProvider services)
	{
		using var scope = services.CreateScope();

		// create database if not exists
		var dbContext = scope.ServiceProvider.GetRequiredService<SecurityContext>();
		if (dbContext.Database.ProviderName!.Equals("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.InvariantCultureIgnoreCase))
		{
			// await dbContext.Database.EnsureCreatedAsync();
			try
			{
				await dbContext.Database.MigrateAsync();
			}
			catch { }
		}

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
