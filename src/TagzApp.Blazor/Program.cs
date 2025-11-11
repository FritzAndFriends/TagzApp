global using TagzApp.Security;
using BlazorDownloadFile;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TagzApp.Blazor.Hubs;
using TagzApp.Blazor.Services;
using TagzApp.Communication.Extensions;
using TagzApp.Configuration.AzureKeyVault;

using KeyVaultExtensions = TagzApp.Configuration.AzureKeyVault.KeyVaultExtensions;

namespace TagzApp.Blazor;

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

		// Configure the Aspire provided service defaults
		builder.AddServiceDefaults();

		builder.Services.SetKeyVaultOptions(builder.Configuration, builder.Environment.IsDevelopment());

		var configure = ConfigureTagzAppFactory.Create(
			builder.Configuration,
			builder.Services.BuildServiceProvider(),
			KeyVaultExtensions.AddAzureKeyVaultConfiguration);
		builder.Services.AddSingleton<IConfigureTagzApp>(configure);

		// Add OpenTelemetry for tracing and metrics.
		// NOTE: Shifting to using the Aspire service defaults
		//builder.Services.AddOpenTelemetryObservability(builder.Configuration);

		var appConfig = await ApplicationConfiguration.LoadFromConfiguration(configure);
		builder.Services.AddSingleton(appConfig);

		var modalConfig = await ModalConfiguration.LoadFromConfiguration(configure);
		builder.Services.AddSingleton(modalConfig);

		var wordFilterConfig = await WordFilterConfiguration.LoadFromConfiguration(configure);
		builder.Services.AddSingleton(wordFilterConfig);

		// Add services to the container.
		builder.Services.AddRazorComponents()
				.AddInteractiveServerComponents()
				.AddInteractiveWebAssemblyComponents();

		await builder.AddTagzAppSecurity(configure, builder.Configuration);
		builder.Services.AddQuickGridEntityFrameworkAdapter();

		//		await Console.Out.WriteLineAsync($">> TagzApp configured: {ConfigureTagzAppFactory.IsConfigured}");

		builder.Services.AddSignalR();
		builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);

		builder.Services.AddHttpClientPolicies();
		await builder.Services.AddTagzAppProviders();

		await builder.AddTagzAppHostedServices(configure);

		// Configure the forwarded headers to allow Container hosting support
		builder.Services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders = ForwardedHeaders.All;
			// Only loopback proxies are allowed by default.
			// Clear that restriction because forwarders are enabled by explicit
			// configuration.
			options.KnownNetworks.Clear();
			options.KnownProxies.Clear();
			
			// Ensure that we trust X-Forwarded-Proto headers for HTTPS detection
			// This is critical for proper OAuth redirect URI generation in containers/proxies
			options.ForwardedProtoHeaderName = "X-Forwarded-Proto";
		});

		builder.Services.AddResponseCompression(options =>
		{
			options.EnableForHttps = true;
		});

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseWebAssemblyDebugging();
			app.UseMigrationsEndPoint();
		}
		else
		{
			app.UseExceptionHandler("/Error", createScopeForErrors: true);
			app.UseStatusCodePagesWithReExecute("/Error/{0}");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
			app.UseResponseCompression();
		}

		// CRITICAL: Use forwarded headers middleware for container/proxy deployments
		// This must be called early in the pipeline to properly detect HTTPS from proxy headers
		app.UseForwardedHeaders();

		app.UseHttpsRedirection();

		app.UseStaticFiles();
		// running in single-user mode -- the current user is an admin
		app.Use(async (context, next) =>
		{

			if (appConfig.SingleUserMode)
			{

				context.User = new ClaimsPrincipal(
					new ClaimsIdentity(new[] {
						new Claim(ClaimTypes.Name, "Admin User"),
						new Claim(ClaimTypes.NameIdentifier, "admin-user"),
						new Claim("DisplayName", "Admin User"),
						new Claim(ClaimTypes.Role, RolesAndPolicies.Role.Admin)
					}, IdentityConstants.ApplicationScheme));
			}

			await next();

		});

		// Ensure OAuth authentication callbacks always use HTTPS
		app.Use(async (context, next) =>
		{
			// For OAuth callback paths, ensure the request is treated as HTTPS
			if (context.Request.Path.StartsWithSegments("/signin-") || 
			    context.Request.Path.StartsWithSegments("/Account"))
			{
				// Override the scheme to HTTPS for OAuth processing
				// This ensures that redirect URIs are always generated with HTTPS
				if (!context.Request.IsHttps)
				{
					context.Request.Scheme = "https";
				}
			}
			
			await next();
		});

		app.UseAuthentication();
		app.UseAuthorization();


		app.UseAntiforgery();
		app.UseMiddleware<DynamicAuthMiddleware>();

		app.MapRazorComponents<TagzApp.Blazor.Components.App>()
				.AddInteractiveServerRenderMode()
				.AddInteractiveWebAssemblyRenderMode()
				.AddAdditionalAssemblies(typeof(TagzApp.Blazor.Client._Imports).Assembly);

		// Add additional endpoints required by the Identity /Account Razor components.
		app.MapAdditionalIdentityEndpoints();

		app.MapHub<MessageHub>("/messages");
		app.MapHub<ModerationHub>("/mod");

		await app.RunAsync(_Source.Token);

	}
}
