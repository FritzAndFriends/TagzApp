using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TagzApp.Web.Data;

public class SecurityContext : IdentityDbContext<TagzAppUser>
{
	private readonly IConfiguration _Configuration;

	public SecurityContext() { }

	public SecurityContext(DbContextOptions<SecurityContext> options, IConfiguration configuration)
			: base(options)
	{
		_Configuration = configuration;
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{

		base.OnModelCreating(builder);
		// Customize the ASP.NET Identity model and override the defaults if needed.
		// For example, you can rename the ASP.NET Identity table names and more.
		// Add your customizations after calling base.OnModelCreating(builder);
	}

	public DbSet<Settings> Settings => Set<Settings>();

}
