// namespace TagzApp.WebTest;

using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Playwright.NUnit;
using Microsoft.AspNetCore.Http.Features;

[SetUpFixture]
public class AssemblyStart
{

	public static WebApplication App { get; private set; }
	public static Task _RunningApp = Task.CompletedTask;

	//[OneTimeSetUp]
	public async Task InitializeAsync()
	{
	
		App = TagzApp.Web.Program.BuildServer(new string[] { "--urls", "http://0.0.0.0:5000" });	
		await App.StartAsync();

	}

	//[OneTimeTearDown]
	public async Task CleanupAsync()
	{
		await App.StopAsync();
	}

}


public class MyBaseFixture : PageTest
{

	private WebApplication? _host;

	protected Uri RootUri { get; private set; } = default!;

	[SetUp]
	public async Task SetUpWebApplication()
	{
		_host = TagzApp.Web.Program.BuildServer(new string[] { });

		await _host.StartAsync();

		RootUri = new(_host.Services.GetRequiredService<IServer>().Features
				.GetRequiredFeature<IServerAddressesFeature>()
				.Addresses.Single());

    await Console.Out.WriteLineAsync("Server is running");
    await Task.Delay(30000);

	}

	[TearDown]
	public async Task TearDownWebApplication()
	{

		//Console.WriteLine("Shutting down in 30s");

		//await Task.Delay(30000);

		if (_host is not null)
		{
			await _host.StopAsync();
			await _host.DisposeAsync();
		}
	}


}