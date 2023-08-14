using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using TagzApp.Web;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

public static class ServicesExtensions {

	public static IServiceCollection AddTagzAppHostedServices(this IServiceCollection services, IConfigurationRoot configuration)
	{

		services.AddSingleton<InMemoryMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<InMemoryMessagingService>());

		return services;

	}

	public static AuthenticationBuilder AddExtenalProviders(this AuthenticationBuilder builder, IConfiguration configuration)
	{

		if (!string.IsNullOrEmpty(configuration["Authentication:Microsoft:ClientId"]))
		{

			builder.AddMicrosoftAccount(microsoftOptions =>
			{
				microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
				microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
			});

		}

		if (!string.IsNullOrEmpty(configuration["Authentication:GitHub:ClientId"]))
		{

			builder.AddGitHub(ghOptions =>
			{
				ghOptions.ClientId = configuration["Authentication:GitHub:ClientId"]!;
				ghOptions.ClientSecret = configuration["Authentication:GitHub:ClientSecret"]!;
			});

		}

		if (!string.IsNullOrEmpty(configuration["Authentication:LinkedIn:ClientId"]))
		{

			builder.AddLinkedIn(liOptions =>
			{
				liOptions.ClientId = configuration["Authentication:LinkedIn:ClientId"]!;
				liOptions.ClientSecret = configuration["Authentication:LinkedIn:ClientSecret"]!;
			});

		}

		return builder;

	}

	public static async Task InitializeSecurity(this WebApplicationBuilder builder, IServiceProvider services)
	{

		using (var scope = services.CreateScope())
		{

			// create database if not exists
			var dbContext = services.GetRequiredService<SecurityContext>();
			await dbContext.Database.EnsureCreatedAsync();

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

	}

}