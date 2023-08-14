using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
  private TestServer? _TestServer;
  private IPlaywright? _Playwright;
  private IBrowser? _Browser;
  private IPage? _StaticPage;
  private string? _Uri;
  private readonly IMessageSink _Output;

  private static int _NextPort = 0;
  private bool _Headless = true;

  public string? Uri => _Uri;

  protected virtual string? Environment { get; } = "Development";

  public PlaywrightWebApplicationFactory(IMessageSink output) => this._Output = output;

  [MemberNotNull(nameof(_Uri))]
  protected override IHost CreateHost(IHostBuilder builder)
  {

    if (Environment is not null)
    {
      builder.UseEnvironment(Environment);
    }
    builder.ConfigureLogging(logging =>
    {
      logging.SetMinimumLevel(LogLevel.Trace);
      logging.AddProvider(new MessageSinkProvider(_Output));
    });

    // We randomize the server port so we ensure that any hard coded Uri's fail in the tests.
    // This also allows multiple servers to run during the tests.
    var port = 5000 + Interlocked.Add(ref _NextPort, 10 + System.Random.Shared.Next(10));
    _Uri = $"http://localhost:{port}";

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
      _Uri = serverAddress;
    }
    else
    {
      var message = new Xunit.Sdk.DiagnosticMessage("Could not get server address from IServerAddressesFeature");
      _Output.OnMessage(message);
    }
  }

  public async Task<IPage> CreatePlaywrightPageAsync(bool headless = true)
  {
    // This method should instantiate a new browser and page for each test.
    // Fix for when browser was not launching in visible mode (headless=false)
    // TODO: determine what configuration for Browser Type & Browser Page options should be used (how to pass in and implement without bloating parameters) 
    _Headless = headless;
    await GetBrowser(headless);

    // TODO: For future BrowserNewPageOptions can affect items like Viewport, ScreenSize size, ignoreHttpsErrors etc.
    // Ref: https://playwright.dev/dotnet/docs/api/class-browser#browser-new-page
    var browserNewPageOptions = new BrowserNewPageOptions()
    {
      BaseURL = _Uri
    };
    return await _Browser.NewPageAsync(browserNewPageOptions);
  }

  public async Task<IPage> CreateSingletonPlaywrightPageAsync(bool headless = true)
  {
    // This method should instantiate a page for all tests in a class where tests dependencies and use same browser page.
    // TODO Discuss whether this needs to be a singleton or implemented differently
    if (_StaticPage is null) { _StaticPage = await CreatePlaywrightPageAsync(headless); }
    return _StaticPage;
  }

  [MemberNotNull(nameof(_Browser))]
  public async Task<IBrowser> GetBrowser(bool headless = true, bool devtools = false)
  {
    // TODO: For future BrowserTypeLaunchOptions typeOptions can also effect type of Browser to launch
    // (We are only using Chromium for now) Note: Channel options exist for MSedge and Chrome also
    // Also, consider SlowMo, Timeout options
    // Refs:
    // https://playwright.dev/dotnet/docs/api/class-browsertype#browsertypelaunchoptions
    // Future Guide for runtime configs: https://playwright.dev/dotnet/docs/next/browsers

    var typeOptions = new BrowserTypeLaunchOptions() { Headless = headless, Devtools = devtools };
    if (_Browser is null)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      // Browser Type gets cached as part of the app
      _Browser ??= (await _Playwright.Chromium.LaunchAsync(typeOptions)) ?? throw new InvalidOperationException();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    return _Browser;
  }

  [MemberNotNull(nameof(_TestServer), nameof(_Playwright))]
  public async Task InitializeAsync()
  {
    // Test classes which inherit IClassFixture<PlaywrightWebApplicationFactory> will have this called automatically.
    // This method should only ever need to be called once as part of the xUnit test lifecycle.
    _TestServer = Server; // Ensures Server (TestServer WebApplicationFactory<T>) is initialized

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
    _Playwright ??= (await Playwright.CreateAsync()) ?? throw new InvalidOperationException();
    //_Browser ??= (await _Playwright.Chromium.LaunchAsync(new() { Headless = _Headless, Devtools = true })) ?? throw new InvalidOperationException();
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    if (_Browser is not null)
    {
      await _Browser.DisposeAsync();
    }
    _Browser = null;
    _Playwright?.Dispose();
    _Playwright = null;
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