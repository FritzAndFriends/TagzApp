using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace TagzApp.WebTest;

/// <summary>
/// WebApplicationFactory that wraps the TestHost in a Kestrel server.  
/// 
/// 
/// <p>
/// Credit to <a href="https://github.com/CZEMacLeod">https://github.com/CZEMacLeod</a> for writing this.
/// </p>
/// </summary>
public class PlaywrightWebApplicationFactory : WebApplicationFactory<Web.Program>, IAsyncLifetime
{
	private IPlaywright? playwright;
	private IBrowser? browser;
	private string? uri;
	private readonly IMessageSink output;

	private static int nextPort = 0;
	private bool _Headless = true;

	public string? Uri => uri;

	protected virtual string? Environment { get; } = "Development";

	public PlaywrightWebApplicationFactory(IMessageSink output) => this.output = output;

	[MemberNotNull(nameof(uri))]
	protected override IHost CreateHost(IHostBuilder builder)
	{

		ServicesExtensions.SocialMediaProviders = new List<IConfigureProvider> { new StartStubSocialMediaProvider() };

		if (Environment is not null)
		{
			builder.UseEnvironment(Environment);
		}
		builder.ConfigureLogging(logging =>
		{
			logging.SetMinimumLevel(LogLevel.Trace);
			logging.AddProvider(new MessageSinkProvider(output));
		});

		// We randomize the server port so we ensure that any hard coded Uri's fail in the tests.
		// This also allows multiple servers to run during the tests.
		var port = 5000 + Interlocked.Add(ref nextPort, 10 + System.Random.Shared.Next(10));
		uri = $"http://localhost:{port}";

		// We the testHost, which can be used with HttpClient with a custom transport
		// It is assumed that the return of CreateHost is a host based on the TestHost Server.
		var testHost = base.CreateHost(builder);

		// Now we reconfigure the builder to use kestrel so we have an http listener that can be used by playwright
		builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel(options =>
		{
			options.ListenLocalhost(port);
		}));
		var host = base.CreateHost(builder);

		UpdateUriFromHost(host);    // For some reason, the kestrel server host does not seem to return the addresses.

		return new CompositeHost(testHost, host);
	}

	private void UpdateUriFromHost(IHost host)
	{
		var server = host.Services.GetRequiredService<IServer>();
		var addresses = server.Features.Get<IServerAddressesFeature>() ?? throw new NullReferenceException("Could not get IServerAddressesFeature");
		var serverAddress = addresses.Addresses.FirstOrDefault();

		if (serverAddress is not null)
		{
			uri = serverAddress;
		}
		else
		{
			var message = new Xunit.Sdk.DiagnosticMessage("Could not get server address from IServerAddressesFeature");
			output.OnMessage(message);
		}
	}

	public async Task<IPage> CreatePlaywrightPageAsync(bool headless = true)
	{

		_Headless = headless;

		var server = Server;        // Ensure Server is initialized
		await InitializeAsync();    // Ensure Playwright is initialized

		return await browser.NewPageAsync(new BrowserNewPageOptions()
		{
			BaseURL = uri
		});
	}

	[MemberNotNull(nameof(playwright), nameof(browser))]
	public async Task InitializeAsync()
	{

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
		playwright ??= (await Playwright.CreateAsync()) ?? throw new InvalidOperationException();
		browser ??= (await playwright.Chromium.LaunchAsync(new() { Headless = _Headless, Devtools = true })) ?? throw new InvalidOperationException();
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

	}

	async Task IAsyncLifetime.DisposeAsync()
	{
		if (browser is not null)
		{
			await browser.DisposeAsync();
		}
		browser = null;
		playwright?.Dispose();
		playwright = null;
	}

	// CompositeHost is based on https://github.com/xaviersolau/DevArticles/blob/e2e_test_blazor_with_playwright/MyBlazorApp/MyAppTests/WebTestingHostFactory.cs
	// Relay the call to both test host and kestrel host.
	public class CompositeHost : IHost
	{
		private readonly IHost testHost;
		private readonly IHost kestrelHost;
		public CompositeHost(IHost testHost, IHost kestrelHost)
		{
			this.testHost = testHost;
			this.kestrelHost = kestrelHost;
		}
		public IServiceProvider Services => testHost.Services;
		public void Dispose()
		{
			testHost.Dispose();
			kestrelHost.Dispose();
		}
		public async Task StartAsync(CancellationToken cancellationToken = default)
		{
			await testHost.StartAsync(cancellationToken);
			await kestrelHost.StartAsync(cancellationToken);
		}
		public async Task StopAsync(CancellationToken cancellationToken = default)
		{
			await testHost.StopAsync(cancellationToken);
			await kestrelHost.StopAsync(cancellationToken);
		}
	}

	private class MessageSinkProvider : ILoggerProvider
	{
		private IMessageSink? output;

		private readonly ConcurrentDictionary<string, ILogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

		public MessageSinkProvider(IMessageSink output) => this.output = output;

		public ILogger CreateLogger(string categoryName) =>
				_loggers.GetOrAdd(categoryName, name => output is null ? NullLogger.Instance : new MessageSinkLogger(name, output));

		protected virtual void Dispose(bool disposing) { output = null; }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private class MessageSinkLogger : ILogger
		{
			private string name;
			private IMessageSink output;

			public MessageSinkLogger(string name, IMessageSink output)
			{
				this.name = name;
				this.output = output;
			}

			public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

			public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			{
				var message = new Xunit.Sdk.DiagnosticMessage(name + ":" + formatter(state, exception));
				output.OnMessage(message);
			}
		}
	}
}