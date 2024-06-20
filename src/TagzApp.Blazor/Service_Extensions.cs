using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TagzApp.Blazor.Client.Services;
using TagzApp.Blazor.Components.Account;
using TagzApp.Blazor.Services;

namespace TagzApp.Blazor;

public static class Service_Extensions
{

	public static async Task<IServiceCollection> AddTagzAppHostedServices(this WebApplicationBuilder builder, IConfigureTagzApp configureTagzApp)
	{

		// TODO: Convert to a notification pipeline
		builder.Services.AddSingleton<INotifyNewMessages, SignalRNotifier>();
		builder.Services.AddScoped<ToastService>();

		builder.Services.AddMemoryCache();

		// Get the content configuration bits
		var connectionSettings = await configureTagzApp.GetConfigurationById<ConnectionSettings>(ConnectionSettings.ConfigurationKey);

		if (connectionSettings.ContentProvider.Equals("postgres", StringComparison.InvariantCultureIgnoreCase))
		{
			builder.AddPostgresServices(configureTagzApp, connectionSettings);
		}
		else
		{
			builder.Services.AddSingleton<IMessagingService, InMemoryMessagingService>();
			builder.Services.AddHostedService(s => s.GetRequiredService<IMessagingService>());
		}

		return builder.Services;
	}



	public static async Task<IServiceCollection> AddTagzAppSecurity(this IHostApplicationBuilder builder, IConfigureTagzApp configure, IConfiguration configuration)
	{

		if (ConfigureTagzAppFactory.IsConfigured)
		{

			// Stash a copy of the configuration in the services collection
			builder.Services.AddSingleton(configure);

			builder.Services.AddCascadingAuthenticationState();
			builder.Services.AddScoped<IdentityUserAccessor>();
			builder.Services.AddScoped<IdentityRedirectManager>();
			builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultScheme = IdentityConstants.ApplicationScheme;
				options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
			})
					.AddExternalProviders(configuration)
					.AddIdentityCookies();

			await builder.AddSecurityContext(configure);
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddDataProtection()
				.SetApplicationName("TagzApp")
				.SetDefaultKeyLifetime(TimeSpan.FromDays(90))
				.PersistKeysToDbContext<SecurityContext>();

			builder.Services.AddIdentityCore<TagzAppUser>(options =>
									options.SignIn.RequireConfirmedAccount = true
							)
							.AddRoles<IdentityRole>()
							.AddEntityFrameworkStores<SecurityContext>()
							.AddSignInManager()
							.AddDefaultTokenProviders();

			builder.Services.AddSingleton<IEmailSender<TagzAppUser>, IdentityNoOpEmailSender>();


		}

		builder.Services.AddAuthorization(config =>
		{
			config.AddPolicy(RolesAndPolicies.Policy.AdminRoleOnly, policy => { policy.RequireRole(RolesAndPolicies.Role.Admin); });
			config.AddPolicy(RolesAndPolicies.Policy.Moderator,
							policy => { policy.RequireRole(RolesAndPolicies.Role.Moderator, RolesAndPolicies.Role.Admin); });
		});

		return builder.Services;

	}

	public static async Task AddSecurityContext(this IHostApplicationBuilder builder, IConfigureTagzApp configuration)
	{

		var connectionSettings = await configuration.GetConfigurationById<ConnectionSettings>(ConnectionSettings.ConfigurationKey);
		if (connectionSettings.SecurityProvider.Equals("postgres", StringComparison.InvariantCultureIgnoreCase))
		{

			builder.AddPostgresSecurityServices(connectionSettings);

		}

		// NOTE: In web app, only using Postgres

		//else if (connectionSettings.SecurityProvider.Equals("sqlite", StringComparison.InvariantCultureIgnoreCase))
		//{

		//	services.AddDbContext<SecurityContext>(options =>
		//	{
		//		options.UseSqlite(connectionSettings.SecurityConnectionString, opt =>
		//		{
		//			opt.MigrationsAssembly(typeof(TagzApp.Storage.Sqlite.Security.SecurityContextModelSnapshot).Assembly.FullName);
		//		});
		//	});

		//	var serviceLocator = services.BuildServiceProvider();
		//	var securityContext = serviceLocator.GetRequiredService<TagzApp.Security.SecurityContext>();
		//	try
		//	{
		//		securityContext.Database.Migrate();
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine($"Error while migrating security context to Postgres: {ex}");
		//	}

		//}
		//else
		//{

		//	// Add the in-memory provider
		//	services.AddDbContext<SecurityContext>(options =>
		//	{
		//		options.UseInMemoryDatabase("tagzapp");
		//	});

		//}

	}


}
