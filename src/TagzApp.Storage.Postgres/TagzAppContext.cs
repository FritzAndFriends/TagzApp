using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

internal class TagzAppContext : DbContext
{

	public TagzAppContext() { }

	public TagzAppContext(DbContextOptions options) : base(options)
	{
	}

	public DbSet<PgContent> Content { get; set; }

	public DbSet<Tag> TagsWatched { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{

		modelBuilder.Entity<PgContent>().HasAlternateKey(c => new { c.Provider, c.ProviderId });

		base.OnModelCreating(modelBuilder);
	}

}
