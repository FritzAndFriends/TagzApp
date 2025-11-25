using TagzApp.Common;

namespace TagzApp.AppHost;
public static class DatabaseConfig
{


	public static IDistributedApplicationBuilder AddDatabase(
		this IDistributedApplicationBuilder builder
		, out IResourceBuilder<PostgresDatabaseResource> db
		, out IResourceBuilder<PostgresDatabaseResource> securityDb
		, out IResourceBuilder<ProjectResource> migration)
	{

		var dbPassword = builder.AddParameter("dbPassword", false);

		var dbServer = builder.AddPostgres("dbServer", password: dbPassword)
			.WithImageTag("16.4")
			.WithPgAdmin()
			.WithDataVolume("tagzapp-dev");

		db = dbServer.AddDatabase(Services.Database.TAGZAPP);

		securityDb = dbServer.AddDatabase(Services.Database.SECURITY);

		migration = builder.AddProject<Projects.TagzApp_MigrationService>("db-migrations")
			.WaitFor(db)
			.WaitFor(securityDb)
			.WithReference(db)
			.WithReference(securityDb);

		return builder;

	}


}
