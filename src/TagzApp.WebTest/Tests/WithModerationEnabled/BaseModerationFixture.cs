using C3D.Extensions.Playwright.AspNetCore.Xunit;
using TagzApp.WebTest.Fixtures;

namespace TagzApp.WebTest.Tests.WithModerationEnabled;

// This fixture creates a new web application, new browser, and a single page for the lifetime of the fixture
// The fixture is in context for the duration of all the tests in a single class.
public class BaseModerationFixture : PlaywrightPageFixture<Web.Program>
{
	public BaseModerationFixture(IMessageSink output) : base(output)
	{
	}

	public bool SkipTest { get; set; }

	private readonly Guid _Uniqueid = Guid.NewGuid();

	protected override IHost CreateHost(IHostBuilder builder)
	{

		builder.AddTestConfiguration(jsonConfiguration: CONFIGURATION);
		builder.UseOnlyStubSocialMediaProvider();
		builder.UseOnlyInMemoryService();
		builder.UseUniqueDb(_Uniqueid);
		return base.CreateHost(builder);
	}

	private const string CONFIGURATION = """
		{
			"ModerationEnabled": "true"
		}
	""";

	// Temp hack to see if it is a timing issue in github actions
	public async override Task InitializeAsync()
	{
		await base.InitializeAsync();
		await Services.ApplyStartUpDelay();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize",
		Justification = "Base class calls SuppressFinalize")]
	public async override ValueTask DisposeAsync()
	{
		await base.DisposeAsync();

		var logger = MessageSink.CreateLogger<PlaywrightFixture>();
		await _Uniqueid.CleanUpDbFilesAsync(logger);
	}
}
