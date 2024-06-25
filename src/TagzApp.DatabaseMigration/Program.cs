using Aspireify.Data.MigrationService;
using Microsoft.EntityFrameworkCore;
using TagzApp.Security;
using TagzApp.Storage.Postgres;
using TagzApp.Storage.Postgres.Security.Migrations;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<SecurityContext>(
	options => options.UseNpgsql(builder.Configuration.GetConnectionString("securitydb"), options => {
		options.MigrationsAssembly(typeof(SecurityContextModelSnapshot).Assembly.FullName);
	})
);

builder.Services.AddDbContext<TagzAppContext>(
	options => options.UseNpgsql(builder.Configuration.GetConnectionString("tagzappdb"))
);

builder.Services.AddOpenTelemetry()
		.WithTracing(tracing => tracing.AddSource(Worker.ActivityName));

var host = builder.Build();
host.Run();
