using TagzApp.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDatabase(out var db, out var securityDb);

#region Website

var tagzAppWeb = builder.AddProject<Projects.TagzApp_Blazor>("web")
	.WithReference(db)
	.WithReference(securityDb);

#endregion

builder.Build().Run();


