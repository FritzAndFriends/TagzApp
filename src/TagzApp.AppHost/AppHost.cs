using AzureKeyVaultEmulator.Aspire.Hosting;
using TagzApp.AppHost;
using TagzApp.Common;

var builder = DistributedApplication.CreateBuilder(args);

var keyVaultEnabled = false;

var keyVault = builder.AddAzureKeyVault(Services.KEYVAULT)
		.RunAsEmulator(new KeyVaultEmulatorOptions { Persist = true }, configSectionName: "AzureKeyVault");

builder.AddDatabase(
	out var db,
	out var securityDb,
	out var migration);

var twitchCache = builder.AddRedis(Services.TWITCH_CACHE)
		.WithRedisInsight();

// TwitchRelay is running on Azure, not locally
// var twitchRelay = builder.AddExecutable("twitchrelay",
//		"func", @"..\TagzApp.TwitchRelay", "start", "--verbose", "--port", "7082")
//	.WithHttpEndpoint(7082, 7082, "http", "foo", false)
//	.WithEnvironment("cache", twitchCache.Resource.ConnectionStringExpression)
//	.WithEnvironment("TwitchRedirectUri", "http://localhost:7082/api/twitchcallback");

#region Website

var tagzAppWeb = builder.AddProject<Projects.TagzApp_Blazor>("web", "https")
	//.WaitForCompletion(migration)
	.WaitFor(db)
	.WithReference(db)
	.WithReference(securityDb);

if (keyVaultEnabled)
{
	tagzAppWeb.WaitFor(keyVault)
		.WithReference(keyVault);
}

// Use the production TwitchRelay on Azure (configured in appsettings.json)
// .WithEnvironment("TwitchRelayUri", "http://localhost:7082");

#endregion

builder.Build().Run();


