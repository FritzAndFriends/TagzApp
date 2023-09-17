using C3D.Extensions.Playwright.AspNetCore.Xunit;
using TagzApp.WebTest.Fixtures;

namespace TagzApp.WebTest.Tests;

// This fixture creates a new web application, new browser, and a single page for the lifetime of the fixture
// The fixture is in context for the duration of all the tests in a single class.
public class WhenFirstSearchingForTagsFixture : PlaywrightPageFixture<Web.Program>
{
	public WhenFirstSearchingForTagsFixture(IMessageSink output) : base(output)
	{
	}

	public bool SkipTest { get; set; }

	private readonly Guid _Uniqueid = Guid.NewGuid();

	public override LogLevel MinimumLogLevel => LogLevel.Warning;

	protected override IHost CreateHost(IHostBuilder builder)
	{
		// ServicesExtensions.SocialMediaProviders = new List<IConfigureProvider> { new StartStubSocialMediaProvider() };
		builder.AddTestConfiguration(jsonConfiguration: CONFIGURATION);
		builder.UseOnlyStubSocialMediaProvider();
		builder.UseOnlyInMemoryService();
		builder.UseUniqueDb(_Uniqueid);
		return base.CreateHost(builder);
	}

	private const string CONFIGURATION = """
		{
			"ModerationEnabled": "false"
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


// We don't pull in the default TestsBase class as we have a different fixture in use for these tests.
[TestCaseOrderer(ordererTypeName: "TagzApp.WebTest.PriorityOrderer", ordererAssemblyName: "TagzApp.WebTest")]
public class WhenFirstSearchingForTags : IClassFixture<WhenFirstSearchingForTagsFixture>
{
	private readonly WhenFirstSearchingForTagsFixture WebApp;
	private readonly ITestOutputHelper _OutputHelper;

	public WhenFirstSearchingForTags(WhenFirstSearchingForTagsFixture webapp, ITestOutputHelper outputHelper)
	{
		WebApp = webapp;
		_OutputHelper = outputHelper;
	}

	[Fact(), TestPriority(1)]
	public async Task CanAddHashtags()
	{
		var page = await WebApp.CreatePlaywrightPageAsync();

		// await using var trace = await page.TraceAsync("Can Add Hashtags", true, true, true);

		var searchPage = await page.GotoHashtagSearchPage();
		await searchPage.SearchForHashtag("dotnet");

		var firstHashtagContent = await page.Locator(".hashtags").First.TextContentAsync();
		Assert.Equal("dotnet", firstHashtagContent);
	}

	[Fact(), TestPriority(2)]
	public async Task LoadContentFromSocialMediaProvider()
	{
		var page = await WebApp.CreatePlaywrightPageAsync();

		await using var trace = await page.TraceAsync("Load Content From Social Media Provider", true, true, true);

		await page
			.GotoWaterfallPage();

		await page.Locator("article").First.WaitForAsync(new()
		{
			Timeout = 5000
		});
	}

	[Fact(), TestPriority(3)]
	public async Task ContentShouldBeInDescendingOrder()
	{
		var page = await WebApp.CreatePlaywrightPageAsync();

		await using var trace = await page.TraceAsync("Content Should Be In Descending Order", true, true, true);

		await page
			.GotoWaterfallPage();

		// Stub generates 10 articles.
		await page.WaitForSelectorAsync("article:nth-child(10)", new() { Timeout = 15000 });

		var timeElements = await page.Locator("article").AllAsync();

		var parsedTimes = await Task.WhenAll(timeElements.Select(async te =>
		{
			var dataTimeAttribute = await te.GetAttributeAsync("data-timestamp");
			var success = DateTime.TryParse(dataTimeAttribute, out DateTime time);
			return (success, time);
		}));

		Assert.True(parsedTimes.All(pt => pt.success));
		Assert.Equal(parsedTimes.Select(pt => pt.time).OrderByDescending(pt => pt), parsedTimes.Select(pt => pt.time));
	}
}
