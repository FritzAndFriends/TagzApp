namespace TagzApp.WebTest;

public class OverlayTests : BaseFixture
{
	public OverlayTests(PlaywrightWebApplicationFactory webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper)
	{
	}

	[Fact]
	public async Task ClickMessageOnWaterfallShowsOverlay()
	{

		const string TAG = "dotnet";

		var page = await WebApp.CreatePlaywrightPageAsync();

		await page.GotoAsync("/");

		await page.GetByPlaceholder("New Hashtag").FillAsync(TAG);

		await page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

		await page.Locator("article").WaitForAsync(new()
		{
			Timeout = 5000
		});

		// Browser 2

		var overlayBrowser = await WebApp.GetBrowser();
		try
		{
			var overlayPage = await overlayBrowser.NewPageAsync(new()
			{
				BaseURL = base.WebApp.Uri
			});
			await overlayPage.GotoAsync($"/overlay/{TAG}");

			var overlayContents = await overlayPage.Locator("#overlayDisplay").AllTextContentsAsync();
			Assert.Empty(string.Join('\n',overlayContents.Select(o => o.Trim()).ToArray()));

			// Click the first message on the waterfall display
			await page.Locator("article").First.ClickAsync();

			// Inspect the overlay
			overlayContents = await overlayPage.Locator("#overlayDisplay").AllTextContentsAsync();
			Assert.NotEmpty(overlayContents);

		} finally {
			await overlayBrowser.DisposeAsync();
		}

	}

}
