using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TagzApp.Web.Services;

namespace TagzApp.WebTest.Fixtures;

public static class FixtureExtensions
{
	public static IHostBuilder UseUniqueDb(this IHostBuilder builder, Guid id) =>
		builder.ConfigureAppConfiguration(configuration =>
		{
			var testConfiguration = new Dictionary<string, string?>()
			{
				{ "ConnectionStrings:AppConfigProvider", "InMemory" }
			};
			configuration.AddInMemoryCollection(testConfiguration);
		});

	public static async Task CleanUpDbFilesAsync(this Guid id, ILogger? logger = null)
	{
		logger ??= Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
		// The host should have shutdown here so we can delete the test database files
		await Task.Delay(50);
		var dbFiles = System.IO.Directory.GetFiles(".", $"TagzApp.Web.{id:N}.db*");
		foreach (var dbFile in dbFiles)
		{
			try
			{
				logger.LogInformation("Removing test database file {File}", dbFile);
				System.IO.File.Delete(dbFile);
			}
			catch (Exception e)
			{
				logger.LogWarning("Could not remove test database file {File}: {Reason}", dbFile, e.Message);
			}
		}
	}

	/// <summary>
	/// Add the file provided from the test project to the host app configuration
	/// </summary>
	/// <param name="builder">The IHostBuilder</param>
	/// <param name="fileName">The filename or null (defaults to appsettings.Test.json)</param>
	/// <returns>Returns the IHostBuilder to allow chaining</returns>
	public static IHostBuilder AddTestConfiguration(this IHostBuilder builder, string? fileName = null, string jsonConfiguration = "{}")
	{
		var testDirectory = System.IO.Directory.GetCurrentDirectory();
		builder.ConfigureAppConfiguration(host =>
		{
			host.AddJsonFile(System.IO.Path.Combine(testDirectory, fileName ?? "appsettings.Test.json"), true);
			var jsonStream = Encoding.UTF8.GetBytes(jsonConfiguration);
			host.AddJsonStream(new MemoryStream(jsonStream));
		});

		return builder;
	}

	/// <summary>
	/// Applies a startup delay based on the configuration parameter TestHostStartDelay. This allows easy adding of a custom delay on build / test servers.
	/// </summary>
	/// <param name="serviceProvider">The IServiceProvider used to get the IConfiguration</param>
	/// <remarks>The default delay if no value is found is 0 and no delay is applied.</remarks>
	public static async Task ApplyStartUpDelay(this IServiceProvider serviceProvider)
	{
		var config = serviceProvider.GetRequiredService<IConfiguration>();
		if (int.TryParse(config["TestHostStartDelay"] ?? "0", out var delay) && delay != 0)
		{
			await Task.Delay(delay);
		}
	}
	public static IEnumerable<KeyValuePair<string, string>> BasicAuthHeaders(this PlaywrightFixture _, string role)
	{
		var rawUserPassword = Encoding.UTF8.GetBytes($"{role}:{role}");
		var base64UserPassword = Convert.ToBase64String(rawUserPassword);
		var auth = new AuthenticationHeaderValue(scheme: BasicAuthenticationDefaults.AuthenticationScheme, base64UserPassword);
		yield return new KeyValuePair<string, string>("Authorization", auth.ToString());
	}

	public static async Task<C3D.Extensions.Playwright.AspNetCore.Utilities.PlaywrightContextPage> CreateAuthorisedPlaywrightBrowserPageAsync(
		this PlaywrightFixture fixture, string role) =>
		await fixture.CreatePlaywrightContextPageAsync(contextOptions: options => options.ExtraHTTPHeaders = fixture.BasicAuthHeaders(role));

	/// <summary>
	/// Registers a basic authentication scheme that succeeds for password==username and assigns the role of the username
	/// </summary>
	public static IHostBuilder AddBasicAuthentication(this IHostBuilder builder) =>
		builder.ConfigureServices(services => services.AddBasicAuthentication());

	/// <summary>
	/// Registers a basic authentication scheme that succeeds for password==username and assigns the role of the username
	/// </summary>
	public static IServiceCollection AddBasicAuthentication(this IServiceCollection services)
		=> services
				.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
				.AddBasic(options =>
				{
					options.Realm = "Test Realm";
					options.Events = new BasicAuthenticationEvents
					{
						OnValidateCredentials = async context =>
						{
							if (context.Username == context.Password)
							{

								var claims = new[]
									{
										// Set UserName
										new Claim(
												ClaimTypes.NameIdentifier,
												context.Username,
												ClaimValueTypes.String,
												context.Options.ClaimsIssuer),
										// Set DisplayName
										new Claim(
												ClaimTypes.Name,
												context.Username,
												ClaimValueTypes.String,
												context.Options.ClaimsIssuer)
									};

								// This bit is probably overkill for our testing needs.
								// Simply adding the role, regardless of whether it exists, to the claim is enough to get this to work.
								// But, in case there is anything else added to the system in the future, we lookup the role and any custom claims.
								var roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
								var role = await roleManager.FindByNameAsync(context.Username);
								IList<Claim> roleClaims = (role is not null ? await roleManager.GetClaimsAsync(role) : null) ?? Enumerable.Empty<Claim>().ToList();
								if (role is not null)
								{
									roleClaims.Add(
										new Claim(
												ClaimTypes.Role,
												context.Username,
												ClaimValueTypes.String,
												context.Options.ClaimsIssuer));
								}

								context.Principal = new ClaimsPrincipal(
										new ClaimsIdentity(claims.Concat(roleClaims), context.Scheme.Name));
								context.Success();
							}
						}
					};
				})
				.Services;
}


public static class InMemoryServiceExtensions
{
	public static IServiceCollection UseOnlyInMemoryService(this IServiceCollection services)
	{
		services.RemoveAll<IMessagingService>();
		//services.AddSingleton<IProviderConfigurationRepository, InMemoryProviderConfigurationRepository>();
		services.AddSingleton<IMessagingService, InMemoryMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<IMessagingService>());
		return services;
	}

	public static IHostBuilder UseOnlyInMemoryService(this IHostBuilder builder) =>
		builder.ConfigureServices(services => services.UseOnlyInMemoryService());

	public static IWebHostBuilder UseOnlyInMemoryService(this IWebHostBuilder builder) =>
		builder.ConfigureServices(services => services.UseOnlyInMemoryService());
}
