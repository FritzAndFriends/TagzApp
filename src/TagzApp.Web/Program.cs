using Microsoft.AspNetCore.Http.Extensions;
using TagzApp.Communication.Extensions;
using TagzApp.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TagzApp.Web.Data;

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
			).AddEntityFrameworkStores<SecurityContext>();

		builder.Services.AddAuthentication()
			.AddMicrosoftAccount(microsoftOptions =>
			{
				microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
				microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
			})
			.AddGitHub(ghOptions =>
			{
				ghOptions.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
				ghOptions.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
			})
		.AddLinkedIn(liOptions =>
		{
			liOptions.ClientId = builder.Configuration["Authentication:LinkedIn:ClientId"]!;
			liOptions.ClientSecret = builder.Configuration["Authentication:LinkedIn:ClientSecret"]!;
		});

		// Add services to the container.
		builder.Services.AddRazorPages();

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

		app.Run();

	}
}