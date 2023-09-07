using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TagzApp.Web.Data;

public class SecurityContext : IdentityDbContext<IdentityUser>
{
	public SecurityContext(DbContextOptions<SecurityContext> options)
			: base(options)
	{
	}

	public DbSet<Settings> Settings => Set<Settings>();

	protected override void OnModelCreating(ModelBuilder builder)
	{

		base.OnModelCreating(builder);
		// Customize the ASP.NET Identity model and override the defaults if needed.
		// For example, you can rename the ASP.NET Identity table names and more.
		// Add your customizations after calling base.OnModelCreating(builder);
	}
}

public record Settings(string Id, string? Value);