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

		Current = EmptyConfigureTagzApp.Instance;

		// Create encryption helper if configuration is available
		EncryptionHelper? encryptionHelper = null;
		try
		{
			var scope = services.CreateScope();
			var logger = scope.ServiceProvider.GetService<ILogger<EncryptionHelper>>();
			encryptionHelper = new EncryptionHelper(configuration, logger);
		}
		catch (Exception)
		{
			// Encryption not configured - will store data in plain text
			// This is acceptable for development or if encryption is not required
		}

		Current = new DbConfigureTagzApp(encryptionHelper);
		var connectionString = configuration.GetConnectionString("tagzappdb");

		try
		{
			Current.InitializeConfiguration("", connectionString ?? throw new InvalidOperationException("Database connection string 'tagzappdb' is not configured"));
			Current.SetConfigurationById<ConnectionSettings>(ConnectionSettings.ConfigurationKey, new ConnectionSettings
			{
				ContentProvider = "postgres",
				SecurityProvider = "postgres",
			}).GetAwaiter().GetResult();
			IsConfigured = true;
		}
		catch (Exception ex)
		{
			// log the exception
			var cfg = EmptyConfigureTagzApp.Instance;

			cfg.Message = ex.InnerException switch
			{
				NpgsqlException => ex.Message,
				_ => $"Unable to initialize database configuration provider"
			};

			Current = cfg;
			IsConfigured = false;

			var scope = services.CreateScope();
			var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbConfigureTagzApp>>();
			logger.LogError(ex, "Unable to initialize configuration provider");

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

		if (jsonObj?["ConnectionStrings"] is null)
		{

			jsonObj!["ConnectionStrings"] = new JsonObject();

		}

		jsonObj!["ConnectionStrings"]!["AppConfigProvider"] = provider;
		jsonObj["ConnectionStrings"]!["AppConfigConnection"] = configurationString;

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
		await Current.InitializeConfiguration(provider, configurationString);
		IsConfigured = true;

	}

}
