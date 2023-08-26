using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

internal class TagzAppContext : DbContext
{

	public TagzAppContext() { }

	public TagzAppContext(DbContextOptions options) : base(options)
	{
	}

	public DbSet<PgContent> Content { get; set; }

}
