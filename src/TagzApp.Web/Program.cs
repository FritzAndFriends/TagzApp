using Microsoft.AspNetCore.Http.Extensions;
using TagzApp.Communication.Extensions;
using TagzApp.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TagzApp.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace TagzApp.Web;

public class Program
{
	private static void Main(string[] args)
	{

		var builder = WebApplication.CreateBuilder(args);
		var connectionString = builder.Configuration.GetConnectionString("SecurityContextConnection") ?? throw new InvalidOperationException("Connection string 'SecurityContextConnection' not found.");

		builder.Services.AddDbContext<SecurityContext>(options => options.UseSqlite(connectionString));

		builder.Services.AddDefaultIdentity<IdentityUser>(options =>
				options.SignIn.RequireConfirmedAccount = true
			)
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<SecurityContext>();

		_ = builder.Services.AddAuthentication().AddExtenalProviders(builder.Configuration);

		builder.Services.AddAuthorization(config =>
		{
			config.AddPolicy(Security.Policy.AdminRoleOnly, policy =>
			{
				policy.RequireRole(Security.Role.Admin);
			});
			config.AddPolicy(Security.Policy.Moderator, policy =>
			{
				policy.RequireRole(Security.Role.Moderator, Security.Role.Admin);
			});
		});

		// Add services to the container.
		builder.Services.AddRazorPages(options => {
			options.Conventions.AuthorizeAreaFolder("Admin", "/", Security.Policy.AdminRoleOnly);
			options.Conventions.AuthorizePage("/Moderation", Security.Policy.Moderator);
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
			app.UseHsts();
		}

		//app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthorization();

		app.MapRazorPages();

		app.MapHub<MessageHub>("/messages");

		if (app.Environment.IsDevelopment())
		{

			var logger = app.Services.GetRequiredService<ILogger<Program>>();
			app.Use(async (ctx, next) =>
			{
				logger.LogInformation("HttpRequest: {Url}", ctx.Request.GetDisplayUrl());
				await next();
			});

		}

		builder.InitializeSecurity(app.Services);

		app.Run();


	}
}
