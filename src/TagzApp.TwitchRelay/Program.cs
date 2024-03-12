using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using TagzApp.TwitchRelay;

var host = new HostBuilder()
		.ConfigureFunctionsWorkerDefaults()
		.ConfigureServices(services =>
		{
			services.AddHttpClient();
			services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(Environment.GetEnvironmentVariable("cache"))));
			services.AddTransient<TwitchProfileRepository>();
			services.AddApplicationInsightsTelemetryWorkerService();
			services.ConfigureFunctionsApplicationInsights();
		})
		.Build();

host.Run();
