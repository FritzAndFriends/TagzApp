using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

namespace TagzApp.Blazor;

public static class OpenTelemetryExtensions
{
	private static readonly Action<ResourceBuilder> _ConfigureResource = r => r.AddService(
		serviceName: "tags-app-blazor",
		serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
		serviceInstanceId: Environment.MachineName);

	public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOpenTelemetry()
			.ConfigureResource(_ConfigureResource)
			.WithTracing(builder =>
			{
				builder.AddSource()
					.SetSampler(new AlwaysOnSampler())
					.AddHttpClientInstrumentation()
					.AddAspNetCoreInstrumentation();

				services.Configure<AspNetCoreTraceInstrumentationOptions>(configuration.GetSection("AspNetCoreInstrumentation"));

				builder.SetupTracingExporter(configuration);
			})
			.WithMetrics(builder =>
			{
				builder
					//.AddMeter() //TODO: Maybe set one up to demonstrate functionality
					.AddRuntimeInstrumentation()
					.AddHttpClientInstrumentation()
					.AddAspNetCoreInstrumentation();

				builder.SetupMetricsView(configuration);
				builder.SetupMetricsExporter(configuration);
			});

		return services;
	}

	public static void AddOpenTelemetryLogging(this ILoggingBuilder builder, IConfiguration configuration)
	{
		var logExporter = configuration.GetValue("UseLogExporter", defaultValue: "console")!.ToLowerInvariant();

		builder.ClearProviders();

		builder.AddOpenTelemetry(options =>
		{
			var resourceBuilder = ResourceBuilder.CreateDefault();
			_ConfigureResource(resourceBuilder);
			options.SetResourceBuilder(resourceBuilder);

			options.SetupLogsExporter(logExporter, configuration);
		});
	}

	private static void SetupTracingExporter(this TracerProviderBuilder builder, IConfiguration configuration)
	{
		var tracingExporter = configuration.GetValue("UseTracingExporter", defaultValue: "console")!.ToLowerInvariant();

		switch (tracingExporter)
		{
			case "otlp":
				builder.AddOtlpExporter(otlpOptions =>
				{
					otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint", defaultValue: " http://localhost:4317")!);
				});
				break;

			default:
				builder.AddConsoleExporter();
				break;
		}
	}

	private static void SetupMetricsExporter(this MeterProviderBuilder builder, IConfiguration configuration)
	{
		var metricsExporter = configuration.GetValue("UseMetricsExporter", defaultValue: "console")!.ToLowerInvariant();

		switch (metricsExporter)
		{
			case "otlp":
				builder.AddOtlpExporter(otlpOptions =>
				{
					otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint", defaultValue: " http://localhost:4317")!);
				});
				break;

			default:
				builder.AddConsoleExporter();
				break;
		}
	}

	private static void SetupMetricsView(this MeterProviderBuilder builder, IConfiguration configuration)
	{
		var histogramAggregation = configuration.GetValue("HistogramAggregation", defaultValue: "explicit")!.ToLowerInvariant();

		switch (histogramAggregation)
		{
			case "exponential":
				builder.AddView(instrument =>
				{
					return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
						? new Base2ExponentialBucketHistogramConfiguration()
						: null;
				});
				break;

			default:
				// Explicit bounds histogram is default, nothing to do.
				break;
		}
	}

	private static void SetupLogsExporter(this OpenTelemetryLoggerOptions options, string logExporter, IConfiguration configuration)
	{
		switch (logExporter)
		{
			case "otlp":
				options.AddOtlpExporter(otlpOptions =>
				{
					otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint", defaultValue: " http://localhost:4317")!);
				});
				break;

			default:
				options.AddConsoleExporter();
				break;
		}
	}
}
