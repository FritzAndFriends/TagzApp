using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TagzApp.Storage.Postgres;

// Design-time factory so that `dotnet ef` can create migrations without needing the full application host
// configuration (KeyVault, custom config providers, etc.). Falls back to a local development connection string
// if the environment variable TAGZAPP_CONTENT_DB is not set.
public class TagzAppContextFactory : IDesignTimeDbContextFactory<TagzAppContext>
{
	public TagzAppContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<TagzAppContext>();

		optionsBuilder.EnableSensitiveDataLogging(); 

		var cs = Environment.GetEnvironmentVariable("TAGZAPP_CONTENT_DB")
						 ?? "Host=localhost;Port=5432;Database=tagzapp;Username=postgres;Password=postgres";

		optionsBuilder.UseNpgsql(cs, npg =>
		{
			// Place for future configuration if needed (migrations assembly, etc.)
		});

		return new TagzAppContext(optionsBuilder.Options);
	}
}
