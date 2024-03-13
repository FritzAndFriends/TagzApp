using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TagzApp.Blazor.Components.Account;
using TagzApp.Blazor.Services;

namespace TagzApp.Blazor;

public static class Service_Extensions
{

	public static async Task<IServiceCollection> AddTagzAppHostedServices(this IServiceCollection services, IConfigureTagzApp configureTagzApp)
	{

		// TODO: Convert to a notification pipeline
		services.AddSingleton<INotifyNewMessages, SignalRNotifier>();

		services.AddMemoryCache();

		// Get the content configuration bits
		var connectionSettings = await configureTagzApp.GetConfigurationById<ConnectionSettings>(ConnectionSettings.ConfigurationKey);

		if (connectionSettings.ContentProvider.Equals("postgres", StringComparison.InvariantCultureIgnoreCase))
		{
			services.AddPostgresServices(configureTagzApp, connectionSettings);
		}
		else
		{
			services.AddSingleton<IMessagingService, InMemoryMessagingService>();
			services.AddHostedService(s => s.GetRequiredService<IMessagingService>());
		}

		return services;
	}



	public static async Task<IServiceCollection> AddTagzAppSecurity(this IServiceCollection services, IConfigureTagzApp configure, IConfiguration configuration)
	{

		if (ConfigureTagzAppFactory.IsConfigured)
		{

			// Stash a copy of the configuration in the services collection
			services.AddSingleton(configure);

			services.AddCascadingAuthenticationState();
			services.AddScoped<IdentityUserAccessor>();
			services.AddScoped<IdentityRedirectManager>();
			services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

			services.AddAuthentication(options =>
			{
				options.DefaultScheme = IdentityConstants.ApplicationScheme;
				options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
			})
					.AddExternalProviders(configuration)
					.AddIdentityCookies();

			await services.AddSecurityContext(configure);
			services.AddDatabaseDeveloperPageExceptionFilter();

			services.AddDataProtection()
				.SetApplicationName("TagzApp")
				.SetDefaultKeyLifetime(TimeSpan.FromDays(90))
				.PersistKeysToDbContext<SecurityContext>();

			services.AddIdentityCore<TagzAppUser>(options =>
									options.SignIn.RequireConfirmedAccount = true
							)
							.AddRoles<IdentityRole>()
							.AddEntityFrameworkStores<SecurityContext>()
							.AddSignInManager()
							.AddDefaultTokenProviders();

			services.AddSingleton<IEmailSender<TagzAppUser>, IdentityNoOpEmailSender>();


		}

		services.AddAuthorization(config =>
		{
			config.AddPolicy(RolesAndPolicies.Policy.AdminRoleOnly, policy => { policy.RequireRole(RolesAndPolicies.Role.Admin); });
			config.AddPolicy(RolesAndPolicies.Policy.Moderator,
							policy => { policy.RequireAuthenticatedUser(); });
		});

		return services;

	}

	public static async Task AddSecurityContext(this IServiceCollection services, IConfigureTagzApp configuration)
	{

		var connectionSettings = await configuration.GetConfigurationById<ConnectionSettings>(ConnectionSettings.ConfigurationKey);
		if (connectionSettings.SecurityProvider.Equals("postgres", StringComparison.InvariantCultureIgnoreCase))
		{

			services.AddPostgresSecurityServices(connectionSettings);

		}
		else if (connectionSettings.SecurityProvider.Equals("sqlite", StringComparison.InvariantCultureIgnoreCase))
		{

			services.AddDbContext<SecurityContext>(options =>
			{
				options.UseSqlite(connectionSettings.SecurityConnectionString, opt =>
				{
					opt.MigrationsAssembly(typeof(TagzApp.Storage.Sqlite.Security.SecurityContextModelSnapshot).Assembly.FullName);
				});
			});

			var serviceLocator = services.BuildServiceProvider();
			var securityContext = serviceLocator.GetRequiredService<TagzApp.Security.SecurityContext>();
			try
			{
				securityContext.Database.Migrate();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error while migrating security context to Postgres: {ex}");
			}

		}
		else
		{

			// Add the in-memory provider
			services.AddDbContext<SecurityContext>(options =>
			{
				options.UseInMemoryDatabase("tagzapp");
			});

		}

	}


}
