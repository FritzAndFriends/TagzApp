// Ignore Spelling: Tagz

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TagzApp.Common;

public static class ConfigureTagzAppFactory
{

	public static bool IsConfigured { get; set; } = false;

	public static IConfigureTagzApp Current = EmptyConfigureTagzApp.Instance;

	public static IConfigureTagzApp Create(IConfiguration configuration, IServiceProvider services)
	{

		if (Current != EmptyConfigureTagzApp.Instance) { return Current; }

		// Get AppConfigConnection and AppConfigProvider from standard IConfiguration in the ConnectionStrings section
		var connectionString = configuration.GetConnectionString("AppConfigConnection");
		var provider = configuration.GetConnectionString("AppConfigProvider");
		Current = EmptyConfigureTagzApp.Instance;

		if (provider?.Equals("inmemory", StringComparison.InvariantCultureIgnoreCase) ?? false)
		{

			CreateInMemoryProvider();

		}
		else if (!string.IsNullOrEmpty(connectionString) &&
			!string.IsNullOrEmpty(provider) &&
			DbConfigureTagzApp.SupportedDbs.Any(db => db.Equals(provider, StringComparison.InvariantCultureIgnoreCase)))
		{

			Current = new DbConfigureTagzApp();

			try
			{
				Current.InitializeConfiguration(provider, connectionString);
				IsConfigured = true;
			}
			catch (Exception ex)
			{
				// log the exception
				var cfg = EmptyConfigureTagzApp.Instance;

				cfg.Message = ex.InnerException switch
				{
					NpgsqlException => ex.Message,
					_ => $"Unable to initialize database configuration provider '{provider}'"
				};

				Current = cfg;
				IsConfigured = false;

				var scope = services.CreateScope();
				var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbConfigureTagzApp>>();
				logger.LogError(ex, "Unable to initialize configuration provider");

			}


		}

		return Current;


	}

	public static void CreateInMemoryProvider()
	{
		Current = new InMemoryConfigureTagzApp();
		IsConfigured = true;
	}

	public static async Task SetConfigurationProvider(string provider, string configurationString)
	{

		var thisFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		var rawJson = await File.ReadAllTextAsync("appsettings.json");
		var jsonObj = JsonNode.Parse(rawJson, documentOptions: new JsonDocumentOptions
		{
			AllowTrailingCommas = true,
			CommentHandling = JsonCommentHandling.Skip
		});

		if (jsonObj["ConnectionStrings"] is null)
		{

			jsonObj["ConnectionStrings"] = new JsonObject();

		}

		jsonObj["ConnectionStrings"]["AppConfigProvider"] = provider;
		jsonObj["ConnectionStrings"]["AppConfigConnection"] = configurationString;

		// update appsettings.json with the new configuration
		using (var file = File.CreateText("appsettings.json"))
		{

			// serialize the jsonObj to the file on disk

			var options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			var outJson = JsonSerializer.Serialize(jsonObj, options);
			await file.WriteAsync(outJson);

		}

		Current = new DbConfigureTagzApp();
		Current.InitializeConfiguration(provider, configurationString);
		IsConfigured = true;

	}

}
