global using TagzApp.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using TagzApp.Web.Hubs;
using TagzApp.Web.Services;

namespace TagzApp.Web;

public class Program
{

	private static CancellationTokenSource _Source = new();

	private static bool _Restarting = true;

	public static bool TestMode { get; set; } = false;

	public static Task Restart()
	{
		_Restarting = true;
		_Source.Cancel();
		return Task.CompletedTask;
	}

	private static async Task Main(string[] args)
	{

		if (TestMode)
		{
			await StartWebsite(args);
		}
		else
		{

			while (_Restarting)
			{

				_Restarting = false;
				_Source = new();

				await StartWebsite(args);

			}

		}

	}

	private static async Task StartWebsite(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var configure = ConfigureTagzAppFactory.Create(builder.Configuration, null);

		var appConfig = await ApplicationConfiguration.LoadFromConfiguration(configure);
		builder.Services.AddSingleton(appConfig);

		// Late bind the connection string so that any changes to the configuration made later on, or in the test fixture can be picked up.
		if (ConfigureTagzAppFactory.IsConfigured)
		{

			// Stash a copy of the configuration in the services collection
			builder.Services.AddSingleton(configure);

			await builder.Services.AddSecurityContext(configure);

			// Add DataProtection services
			builder.Services.AddDataProtection()
				.SetApplicationName("TagzApp")
				.SetDefaultKeyLifetime(TimeSpan.FromDays(90))
				.PersistKeysToDbContext<SecurityContext>();

			builder.Services.AddDefaultIdentity<TagzAppUser>(options =>
									options.SignIn.RequireConfirmedAccount = true
							)
							.AddRoles<IdentityRole>()
							.AddEntityFrameworkStores<SecurityContext>();

			_ = builder.Services.AddAuthentication()
				.AddCookie()
				.AddExternalProviders(builder.Configuration);

			builder.Services.AddAuthorization(config =>
			{
				config.AddPolicy(Security.Policy.AdminRoleOnly, policy => { policy.RequireRole(Security.Role.Admin); });
				config.AddPolicy(Security.Policy.Moderator,
								policy => { policy.RequireRole(Security.Role.Moderator, Security.Role.Admin); });
			});
		}

		builder.Services.AddRazorPages(options =>
		{
			options.Conventions.AuthorizeAreaFolder("Admin", "/", Security.Policy.AdminRoleOnly);
			options.Conventions.AuthorizePage("/Moderation", Security.Policy.Moderator);
			options.Conventions.AuthorizePage("/BlockedUsers", Security.Policy.Moderator);
		});

		// Configure the forwarded headers to allow Container hosting support
		builder.Services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders = ForwardedHeaders.All;
			// Only loopback proxies are allowed by default.
			// Clear that restriction because forwarders are enabled by explicit
			// configuration.
			options.KnownNetworks.Clear();
			options.KnownProxies.Clear();
		});

		await builder.Services.AddTagzAppHostedServices(configure);

		builder.Services.AddSignalR();

		builder.Services.AddHttpLogging(options =>
		{
			options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders;
		});

		// configure TempData serialization with System.Text.Json
		builder.Services.AddSingleton<TempDataSerializer, JsonTempDataSerializer>();

		//// Add the Polly policies
		/// Removing in favor of provider specific Polly Policies
		//builder.Services.AddPolicies();

		builder.Services.AddSingleton<ViewModelUtilitiesService>();

		var app = builder.Build();

		app.UseForwardedHeaders();
		app.UseHttpLogging();
		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}
		else
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseCookiePolicy(new CookiePolicyOptions()
		{
			MinimumSameSitePolicy = SameSiteMode.Lax
		});
		app.UseCertificateForwarding();

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapRazorPages();

		app.UseMiddleware<StartupConfigMiddleware>(app.Configuration);

		app.MapHub<MessageHub>("/messages");
		app.MapHub<ModerationHub>("/mod");

		if (app.Environment.IsDevelopment())
		{
			var logger = app.Services.GetRequiredService<ILogger<Program>>();
			app.Use(async (ctx, next) =>
			{
				logger.LogInformation("HttpRequest: {Url}", ctx.Request.GetDisplayUrl());
				await next();
			});
		}

		if (ConfigureTagzAppFactory.IsConfigured)
		{
			app.Services.InitializeSecurity().GetAwaiter().GetResult(); // Ensure this runs before we start the app.
		}

		await app.RunAsync(_Source.Token);
	}
}
