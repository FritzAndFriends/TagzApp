using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

public class PgSqlContext : DbContext
{

	public PgSqlContext(DbContextOptions<PgSqlContext> options) : base(options)
	{
	}

	public DbSet<Content> Contents { get; set; }
	public DbSet<ModerationAction> ModerationActions { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{

		modelBuilder.Entity<Content>().HasKey(x => new { x.Provider, x.ProviderId });
		modelBuilder.Entity<ModerationAction>().HasKey(x => new { x.Provider, x.ProviderId });
		modelBuilder.Entity<ModerationAction>().HasOne<Content>("Content");

		base.OnModelCreating(modelBuilder);
	}

}
