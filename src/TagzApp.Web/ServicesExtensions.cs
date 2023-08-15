using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web;

public static class ServicesExtensions
{

	public static IServiceCollection ConfigureProvider<T>(this IServiceCollection services, IConfiguration configuration) where T : IConfigureProvider, new()
	{

		var providerStart = (IConfigureProvider)(Activator.CreateInstance<T>());
		providerStart.RegisterServices(services, configuration);

		return services;

	}

	public static IServiceCollection ConfigureProvider(this IServiceCollection services, IConfigureProvider provider, IConfiguration configuration)
	{

		provider.RegisterServices(services, configuration);

		return services;

	}

	public static IServiceCollection AddTagzAppHostedServices(this IServiceCollection services, IConfigurationRoot configuration)
	{

		services.AddSingleton<InMemoryMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<InMemoryMessagingService>());

		// Register the providers
		if (SocialMediaProviders.Any())
		{
			foreach (var item in SocialMediaProviders)
			{
				services.ConfigureProvider(item, configuration);
			}
		}
		else
		{
			services.ConfigureProvider<TagzApp.Providers.Mastodon.StartMastodon>(configuration);
			services.ConfigureProvider<TagzApp.Providers.Twitter.StartTwitter>(configuration);
			services.ConfigureProvider<TagzApp.Providers.TwitchChat.StartTwitchChat>(configuration);
		}

		return services;

	}

	/// <summary>
	/// A collection of externally configured providers
	/// </summary>
	public static List<IConfigureProvider> SocialMediaProviders { get; set; } = new();
  public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name, IConfiguration configuration, 
    Action<IConfiguration> action)
		{
    var section = configuration.GetSection($"Authentication:{name}");
    if (section is not null) action(section);
    return builder;
  }

  public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name, IConfiguration configuration, 
    Action<string, string> action)
			{
    return builder.AddExternalProvider(name, configuration, (section) => {
      var clientID = section["ClientID"];
      var clientSecret = section["ClientSecret"];
      if (!string.IsNullOrEmpty(clientID) && !string.IsNullOrEmpty(clientSecret))
      {
        action(clientID, clientSecret);
      }
			});
		}

  public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name, IConfiguration configuration,
    Action<Action<Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions>> action)
		{
    return builder.AddExternalProvider(name, configuration, (section) => {
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

  public static AuthenticationBuilder AddExternalProviders(this AuthenticationBuilder builder, IConfiguration configuration)
		{

    builder.AddExternalProvider("Microsoft", configuration , options => builder.AddMicrosoftAccount(options));
    builder.AddExternalProvider("GitHub", configuration, options => builder.AddGitHub(options));
    builder.AddExternalProvider("LinkedIn", configuration, options => builder.AddLinkedIn(options));

		return builder;

	}

  public static async Task InitializeSecurity(this IServiceProvider services)
	{

    using var scope = services.CreateScope();

    // create database if not exists
    var dbContext = scope.ServiceProvider.GetRequiredService<SecurityContext>();
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