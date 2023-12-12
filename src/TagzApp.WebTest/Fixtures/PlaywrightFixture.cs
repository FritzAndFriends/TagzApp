using C3D.Extensions.Playwright.AspNetCore.Xunit;

namespace TagzApp.WebTest.Fixtures;

/// <summary>
/// WebApplicationFactory that wraps the TestHost in a Kestrel server and provides Playwright and HttpClient testing. 
/// This also logs output from the Host under test to Xunit.
/// 
/// <p>
/// Credit to <a href="https://github.com/CZEMacLeod">https://github.com/CZEMacLeod</a> for writing this. 
/// Functionality is now wrapped in the nuget package C3D.Extensions.Playwright.AspNetCore.Xunit
/// </p>
/// </summary>
public class PlaywrightFixture : PlaywrightFixture<Web.Program>
{
	public override string? Environment { get; } = "Test";

	public PlaywrightFixture(IMessageSink output) : base(output)
	{
	}

	private readonly Guid _Uniqueid = Guid.NewGuid();

	public override LogLevel MinimumLogLevel => LogLevel.Warning;

	protected override IHost CreateHost(IHostBuilder builder)
	{

		ConfigureTagzAppFactory.CreateInMemoryProvider();
		TagzApp.Web.Program.TestMode = true;


		//ServicesExtensions.SocialMediaProviders = new List<IConfigureProvider> { new StartStubSocialMediaProvider() };
		builder.AddTestConfiguration(jsonConfiguration: CONFIGURATION);
		builder.UseOnlyStubSocialMediaProvider();
		builder.UseOnlyInMemoryService();
		builder.UseUniqueDb(_Uniqueid);
		builder.AddBasicAuthentication();
		var host = base.CreateHost(builder);

		return host;
	}

	private const string CONFIGURATION = """
		{
			"ModerationEnabled": "false"
		}
	""";


	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize",
		Justification = "Base class calls SuppressFinalize")]
	public async override ValueTask DisposeAsync()
	{
		await base.DisposeAsync();

		var logger = MessageSink.CreateLogger<PlaywrightFixture>();
		await _Uniqueid.CleanUpDbFilesAsync(logger);
	}

	// Temp hack to see if it is a timing issue in github actions
	public async override Task InitializeAsync()
	{
		await base.InitializeAsync();
		await Services.ApplyStartUpDelay();
	}
}
