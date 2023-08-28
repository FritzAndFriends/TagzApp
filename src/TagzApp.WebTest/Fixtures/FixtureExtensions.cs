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
				{ "ConnectionStrings:SecurityContextConnection", $"Data Source=TagzApp.Web.{id:N}.db" }
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
	public static IHostBuilder AddTestConfiguration(this IHostBuilder builder, string? fileName = null)
	{
		var testDirectory = System.IO.Directory.GetCurrentDirectory();
		builder.ConfigureAppConfiguration(host =>
			host.AddJsonFile(System.IO.Path.Combine(testDirectory, fileName ?? "appsettings.Test.json"), true));
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
}


public static class InMemoryServiceExtensions
{
	public static IServiceCollection UseOnlyInMemoryService(this IServiceCollection services)
	{
		services.RemoveAll<IMessagingService>();
		services.AddSingleton<IMessagingService, InMemoryMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<IMessagingService>());
		return services;
	}

	public static IHostBuilder UseOnlyInMemoryService(this IHostBuilder builder) =>
		builder.ConfigureServices(services => services.UseOnlyInMemoryService());

	public static IWebHostBuilder UseOnlyInMemoryService(this IWebHostBuilder builder) =>
		builder.ConfigureServices(services => services.UseOnlyInMemoryService());
}

