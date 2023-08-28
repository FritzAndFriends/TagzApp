using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TagzApp.Common.Models;

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

public class PgModerationAction
{

	public long Id { get; set; }

	public required string Provider { get; set; } 

	public required string ProviderId { get; set; }

	public required ModerationState State { get; set; } = ModerationState.Pending;

	[MaxLength(100)]
	public string? Reason { get; set; }

	[MaxLength(100)]
	public string? Moderator { get; set; }

	public required DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

}