﻿using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using TagzApp.Common.Telemetry;

namespace TagzApp.Blazor;

public static class OpenTelemetryExtensions
{
	private static readonly Action<ResourceBuilder> _ConfigureResource = r => r.AddService(
		serviceName: "tagz-app-blazor",
		serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
		serviceInstanceId: Environment.MachineName);

	public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, IConfiguration configuration)
	{

		if (!bool.Parse(configuration["EnableTelemetry"] ?? "true")) return services;

		services.AddOpenTelemetry()
			.ConfigureResource(_ConfigureResource)
			.WithTracing(builder =>
			{
				var resourceBuilder = ResourceBuilder.CreateDefault();
				_ConfigureResource(resourceBuilder);

				builder
					.SetResourceBuilder(resourceBuilder)
					.AddSource()
					.SetSampler(new AlwaysOnSampler())
					.AddHttpClientInstrumentation()
					.AddAspNetCoreInstrumentation();

				services.Configure<AspNetCoreTraceInstrumentationOptions>(configuration.GetSection("AspNetCoreInstrumentation"));

				builder.SetupTracingExporter(configuration);
			})
			.WithMetrics(builder =>
			{
				var resourceBuilder = ResourceBuilder.CreateDefault();
				_ConfigureResource(resourceBuilder);

				builder
					.SetResourceBuilder(resourceBuilder)
					.AddMeter("tagzapp-provider-metrics")
					.AddProcessInstrumentation()
					.AddRuntimeInstrumentation()
					.AddHttpClientInstrumentation()
					.AddAspNetCoreInstrumentation();

				builder.SetupMetricsView(configuration);
				builder.SetupMetricsExporter(configuration);
			});

		services.AddSingleton<ProviderInstrumentation>();

		return services;
	}

	public static void AddOpenTelemetryLogging(this ILoggingBuilder builder, IConfiguration configuration)
	{

		if (!bool.Parse(configuration["EnableTelemetry"] ?? "true")) return;

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
