namespace TagzApp.WebTest.Tests.WithModerationEnabled;



public class WaterfallTests : IClassFixture<BaseModerationFixture>
{

	private IPage Page => _Webapp.Page; //Shared page for all tests
	private readonly BaseModerationFixture _Webapp;
	private readonly ITestOutputHelper _OutputHelper;

	public WaterfallTests(BaseModerationFixture webapp, ITestOutputHelper outputHelper)
	{
		_Webapp = webapp;
		_OutputHelper = outputHelper;
	}

	[Fact(Skip = "Problem getting this to run reliably")]
	public async Task NoUnapprovedContentShouldAppear()
	{
		var page = await _Webapp.CreatePlaywrightPageAsync();

		await using var trace = await page.TraceAsync("No Unapproved content should appear", true, true, true);

		var searchpage = await page.GotoHashtagSearchPage();
		var searchedHashtag = await searchpage.SearchForHashtag("dotnet");
		await searchedHashtag.GotoWaterfallPage();

		await Assert.ThrowsAsync<TimeoutException>(async () =>
		{
			await page.Locator("article").First.WaitForAsync(new()
			{
				Timeout = 1000
			});

			await page.ScreenshotAsync(new()
			{
				Path = "NoUnapprovedContentShouldAppear.png"
			});

		});

	}


}
