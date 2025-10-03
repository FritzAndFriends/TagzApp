using Microsoft.EntityFrameworkCore;

namespace TagzApp.Storage.Postgres;

public class TagzAppContext : DbContext
{

	public TagzAppContext(DbContextOptions options) : base(options)
	{
	}

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//{
	//	optionsBuilder.UseNpgsql();
	//	base.OnConfiguring(optionsBuilder);
	//}

	public DbSet<PgContent> Content { get; set; }

	public DbSet<PgModerationAction> ModerationActions { get; set; }

	public DbSet<PgBlockedUser> BlockedUsers { get; set; }

	//public DbSet<Settings> Settings => Set<Settings>();

	public DbSet<Tag> TagsWatched { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{

		modelBuilder.Entity<PgBlockedUser>()
			.Property(b => b.Capabilities)
			.HasDefaultValue(BlockedUserCapabilities.Moderated);

		modelBuilder.Entity<PgContent>().HasAlternateKey(c => new { c.Provider, c.ProviderId });
		modelBuilder.Entity<PgContent>().HasOne(c => c.ModerationAction).WithOne(m => m.Content).HasForeignKey<PgModerationAction>(m => m.ContentId);
		modelBuilder.Entity<PgContent>().Property(c => c.Timestamp)
			.HasConversion(
				t => t.UtcDateTime,
				t => new DateTimeOffset(t, TimeSpan.Zero)
			);

		// Store Author as jsonb so we can index/query efficiently
		modelBuilder.Entity<PgContent>()
			.Property(c => c.Author)
			.HasColumnType("jsonb");

		// Computed column for AuthorUserName (lower-cased username extracted from JSON Author)
		// Author column is already jsonb, so no cast required. Keep expression aligned with current snapshot to avoid needless migrations.
		modelBuilder.Entity<PgContent>()
			.Property(c => c.AuthorUserName)
			.HasComputedColumnSql("lower((\"Author\" ->> 'UserName'))", stored: true);

		// Index to accelerate lookups by provider + author username + recency
		modelBuilder.Entity<PgContent>()
			.HasIndex(c => new { c.Provider, c.AuthorUserName, c.Timestamp });


		modelBuilder.Entity<PgModerationAction>().HasAlternateKey(c => new { c.Provider, c.ProviderId });

		modelBuilder.Entity<Tag>().Property(t => t.Text)
			.HasMaxLength(50)
			.IsRequired();

		base.OnModelCreating(modelBuilder);

	}

}
