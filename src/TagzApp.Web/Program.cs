using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TagzApp.Communication.Extensions;
using TagzApp.Web.Data;
using TagzApp.Web.Hubs;

namespace TagzApp.Web;

public class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Late bind the connection string so that any changes to the configuration made later on, or in the test fixture can be picked up.
		builder.Services.AddDbContext<SecurityContext>((services, options) =>
			options.UseSqlite(
				services.GetRequiredService<IConfiguration>().GetConnectionString("SecurityContextConnection") ??
				throw new InvalidOperationException("Connection string 'SecurityContextConnection' not found.")));

		builder.Services.AddDefaultIdentity<IdentityUser>(options =>
				options.SignIn.RequireConfirmedAccount = true
			)
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<SecurityContext>();

		_ = builder.Services.AddAuthentication().AddExternalProviders(builder.Configuration);

		builder.Services.AddAuthorization(config =>
		{
			config.AddPolicy(Security.Policy.AdminRoleOnly, policy => { policy.RequireRole(Security.Role.Admin); });
			config.AddPolicy(Security.Policy.Moderator,
				policy => { policy.RequireRole(Security.Role.Moderator, Security.Role.Admin); });
		});

		// Add services to the container.
		builder.Services.AddRazorPages(options =>
		{
			options.Conventions.AuthorizeAreaFolder("Admin", "/", Security.Policy.AdminRoleOnly);
			options.Conventions.AuthorizePage("/Moderation", Security.Policy.Moderator);
		});

		builder.Services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost |
								ForwardedHeaders.XForwardedProto;
			// Only loopback proxies are allowed by default.
			// Clear that restriction because forwarders are enabled by explicit
			// configuration.
			options.KnownNetworks.Clear();
			options.KnownProxies.Clear();
		});

		builder.Services.AddTagzAppHostedServices(builder.Configuration);

		builder.Services.AddSignalR();

		// Add the Polly policies
		builder.Services.AddPolicies(builder.Configuration);

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseForwardedHeaders();
			app.UseHsts();
		} else {
			app.UseDeveloperExceptionPage();
			app.UseForwardedHeaders();
		}

		app.UseCookiePolicy(new CookiePolicyOptions()
		{
			MinimumSameSitePolicy = SameSiteMode.Lax
		});
		app.UseCertificateForwarding();

		//app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthorization();

		app.MapRazorPages();

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

		app.Services.InitializeSecurity().GetAwaiter().GetResult(); // Ensure this runs before we start the app.

		app.Run();
	}
}