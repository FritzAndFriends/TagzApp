using Aspire.Hosting;
using TagzApp.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDatabase(out var db, out var securityDb);

var twitchCache = builder.AddRedis("twitchCache");
var twitchRelay = builder.AddExecutable("twitchrelay",
		"func", @"..\TagzApp.TwitchRelay", "start", "--verbose", "--port", "7082")
	.WithHttpEndpoint(7082, 7082,"http", "foo", false)
	.WithEnvironment("cache", twitchCache.Resource.ConnectionStringExpression)
	.WithEnvironment("TwitchRedirectUri", "http://localhost:7082/api/twitchcallback");

#region Website

var tagzAppWeb = builder.AddProject<Projects.TagzApp_Blazor>("web", "https")
	.WithReference(db)
	.WithReference(securityDb)
	.WithEnvironment("TwitchRelayUri", "http://localhost:7082");

#endregion

builder.Build().Run();


