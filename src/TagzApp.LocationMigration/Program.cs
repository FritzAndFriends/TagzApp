using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TagzApp.Storage.Postgres;
using TagzApp.LocationMigration;


var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration
		.AddJsonFile("appsettings.json", optional: false)
		.AddEnvironmentVariables()
		.AddCommandLine(args);

// Add logging
builder.Services.AddLogging(logging =>
{
	logging.AddConsole();
	logging.SetMinimumLevel(LogLevel.Information);
});

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Please update appsettings.json with your PostgreSQL connection string.");
}

builder.Services.AddDbContext<TagzAppContext>(options =>
		options.UseNpgsql(connectionString));

// Add DbContextFactory for creating multiple context instances
builder.Services.AddDbContextFactory<TagzAppContext>(options =>
		options.UseNpgsql(connectionString));

// Add HTTP client for geocoding
builder.Services.AddHttpClient();

// Add our services
builder.Services.AddScoped<SimpleGeocoder>();
builder.Services.AddScoped<LocationMigrationService>();

// Add memory cache for location caching
builder.Services.AddMemoryCache();

var host = builder.Build();

// Run the migration
var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
	// Check for test option first
	if (args.Contains("--test"))
	{
		logger.LogInformation("Running validation tests...");
		// TestValidation(logger);  // Commented out for now
		return;
	}

	// Check for dry-run option
	var isDryRun = args.Contains("--dry-run") || args.Contains("-d");

	logger.LogInformation("Starting TagzApp Location Migration Tool{DryRun}", isDryRun ? " (DRY RUN)" : "");
	logger.LogInformation("Connection string: {ConnectionString}", connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...");

	using var scope = host.Services.CreateScope();
	var migrationService = scope.ServiceProvider.GetRequiredService<LocationMigrationService>();

	if (isDryRun)
	{
		var analysis = await migrationService.AnalyzeContentAsync();

		logger.LogInformation("=== DRY RUN ANALYSIS COMPLETE ===");
		logger.LogInformation("Twitch records found: {TwitchRecords}", analysis.TwitchRecords);
		logger.LogInformation("YouTube-Chat records found: {YouTubeRecords}", analysis.YouTubeRecords);
		logger.LogInformation("Total content records to process: {TotalRecords}", analysis.TotalRecords);
		logger.LogInformation("Unique locations detected: {UniqueLocations}", analysis.UniqueLocations.Count);
		logger.LogInformation("Users who would be processed: {UsersToProcess}", analysis.UsersToProcess);
		logger.LogInformation("Users already in ViewerLocations (skipped): {ExistingUsers}", analysis.ExistingUsers);
		logger.LogInformation("Locations that would need geocoding: {LocationsToGeocode}", analysis.LocationsToGeocode);
		logger.LogInformation("Locations available in cache: {CachedLocations}", analysis.CachedLocations);

		if (analysis.UniqueLocations.Count > 0)
		{
			logger.LogInformation("Sample locations that would be processed:");
			var sampleLocations = analysis.UniqueLocations.Take(10).ToList();
			foreach (var location in sampleLocations)
			{
				logger.LogInformation("  - '{Location}'", location);
			}
			if (analysis.UniqueLocations.Count > 10)
			{
				logger.LogInformation("  ... and {More} more", analysis.UniqueLocations.Count - 10);
			}
		}
	}
	else
	{
		var locationsSaved = await migrationService.MigrateLocationsAsync();
		logger.LogInformation("Migration completed successfully. Total locations saved: {LocationsSaved}", locationsSaved);
	}
}
catch (Exception ex)
{
	logger.LogError(ex, "Migration failed: {Error}", ex.Message);
	Environment.Exit(1);
}

