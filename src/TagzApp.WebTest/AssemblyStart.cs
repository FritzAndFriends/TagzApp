// namespace TagzApp.WebTest;

[SetUpFixture]
public class AssemblyStart
{

	public static WebApplication App { get; private set; }
	public Task _RunningApp;

	[OneTimeSetUp]
	public async Task InitializeAsync()
	{
	
		App = TagzApp.Web.Program.BuildServer(Array.Empty<string>());	
		_RunningApp = App.RunAsync();

	}

	[OneTimeTearDown]
	public async Task CleanupAsync()
	{
		await App.StopAsync();
	}

}
