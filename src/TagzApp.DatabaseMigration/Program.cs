using Aspireify.Data.MigrationService;
using Aspireify.Data.Security;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<AspireifySecurityContext>(
	options => options.UseNpgsql(builder.Configuration.GetConnectionString("security"))
);

builder.Services.AddOpenTelemetry()
		.WithTracing(tracing => tracing.AddSource(Worker.ActivityName));

var host = builder.Build();
host.Run();
