using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TagzApp.Web.Data;

public class SecurityContext : IdentityDbContext<IdentityUser>
{
	private readonly IConfiguration _Configuration;

	public SecurityContext(DbContextOptions<SecurityContext> options, IConfiguration configuration)
			: base(options)
	{
		_Configuration = configuration;
	}

	public SecurityContext(IConfiguration configuration)
	{
		_Configuration = configuration;
	}

	public DbSet<Settings> Settings => Set<Settings>();

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{

		if (!string.IsNullOrEmpty(_Configuration.GetConnectionString("TagzAppSecurity")))
		{

			optionsBuilder.UseNpgsql(
							_Configuration.GetConnectionString("TagzAppSecurity"),
							pg => pg.MigrationsAssembly("TagzApp.Storage.Postgres.Security"));

		}
		else if (!string.IsNullOrEmpty(_Configuration.GetConnectionString("SecurityContextConnection")))
		{

			optionsBuilder.UseSqlite(
							_Configuration.GetConnectionString("SecurityContextConnection")
			);

		}
		else
		{

			optionsBuilder.UseInMemoryDatabase("InMemoryDatabase");

		}

		base.OnConfiguring(optionsBuilder);
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{

		base.OnModelCreating(builder);
		// Customize the ASP.NET Identity model and override the defaults if needed.
		// For example, you can rename the ASP.NET Identity table names and more.
		// Add your customizations after calling base.OnModelCreating(builder);
	}
}

public record Settings(string Id, string? Value);
