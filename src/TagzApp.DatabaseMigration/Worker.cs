using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using System.Diagnostics;
using TagzApp.Security;
using TagzApp.Storage.Postgres;

namespace Aspireify.Data.MigrationService;

public class Worker : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	private readonly IHostApplicationLifetime hostApplicationLifetime;
	private readonly ILogger<Worker> _logger;

	internal const string ActivityName = "MigrationService";
	private static readonly ActivitySource s_activitySource = new(ActivityName);

	public Worker(IServiceProvider serviceProvider,
		IHostApplicationLifetime hostApplicationLifetime,
		ILogger<Worker> logger)
	{
		this.serviceProvider = serviceProvider;
		this.hostApplicationLifetime = hostApplicationLifetime;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{

		using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

		try
		{
			using var scope = serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<SecurityContext>();

			await dbContext.Database.MigrateAsync(stoppingToken);

		}
		catch (Exception ex)
		{
			activity?.RecordException(ex);
			throw;
		}

		using var tagzActivity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

		try
		{
			using var scope = serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<TagzAppContext>();

			await dbContext.Database.MigrateAsync(stoppingToken);

		}
		catch (Exception ex)
		{
			activity?.RecordException(ex);
			throw;
		}

		hostApplicationLifetime.StopApplication();
	}
}
