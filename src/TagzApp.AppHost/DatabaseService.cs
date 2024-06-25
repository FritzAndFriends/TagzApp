using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			.WithPgAdmin()
			.WithDataVolume("tagzapp-dev");

		db = dbServer.AddDatabase("tagzappdb");

		securityDb = dbServer.AddDatabase("securitydb");


		migration = builder.AddProject<Projects.TagzApp_MigrationService>("db-migrations")
			.WaitFor(db)
			.WaitFor(securityDb)
			.WithReference(db)
			.WithReference(securityDb);

		return builder;

	}


}
