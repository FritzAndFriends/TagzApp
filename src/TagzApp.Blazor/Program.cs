global using TagzApp.Security;
using BlazorDownloadFile;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;
using TagzApp.Blazor;
using TagzApp.Blazor.Hubs;
using TagzApp.Communication.Extensions;

internal class Program
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

		var configure = ConfigureTagzAppFactory.Create(builder.Configuration, builder.Services.BuildServiceProvider());

		// Add OpenTelemetry for tracing and metrics.
		// NOTE: Shifting to using the Aspire service defaults
		//builder.Services.AddOpenTelemetryObservability(builder.Configuration);

		var appConfig = await ApplicationConfiguration.LoadFromConfiguration(configure);
		builder.Services.AddSingleton(appConfig);

		var modalConfig = await ModalConfiguration.LoadFromConfiguration(configure);
		builder.Services.AddSingleton(modalConfig);

		// Add services to the container.
		builder.Services.AddRazorComponents()
				.AddInteractiveServerComponents()
				.AddInteractiveWebAssemblyComponents();

		await builder.AddTagzAppSecurity(configure, builder.Configuration);

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
		});

		// Add OpenTelemetry for logging.
		builder.Logging.AddOpenTelemetryLogging(builder.Configuration);

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
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
			app.UseResponseCompression();
		}

		app.Use((context, next) =>
		{

			// running in single-user mode -- the current user is an admin
			if (appConfig.SingleUserMode)
			{
				context.User = new ClaimsPrincipal(
					new ClaimsIdentity(new[] {
						new Claim(ClaimTypes.Name, "Admin User"),
						new Claim("DisplayName", "Admin User"),
						new Claim(ClaimTypes.Role, RolesAndPolicies.Role.Admin)
					}, "Basic"));
			}

			return next();

		});


		app.UseHttpsRedirection();

		app.UseStaticFiles();
		app.UseAntiforgery();

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
