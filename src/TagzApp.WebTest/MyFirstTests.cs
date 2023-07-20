namespace TagzApp.WebTest;


public class MyFirstTests : IClassFixture<PlaywrightWebApplicationFactory>
{
	private readonly PlaywrightWebApplicationFactory _WebApp;
	private readonly ITestOutputHelper _OutputHelper;

	public MyFirstTests(PlaywrightWebApplicationFactory webapp, ITestOutputHelper outputHelper)
  {
		_WebApp = webapp;
		_OutputHelper = outputHelper;
	}

  [Fact]
	public async Task CanAddHashtags()
	{

		var page = await _WebApp.CreatePlaywrightPageAsync();

		//await page.Context.Tracing.StartAsync(new()
		//{
		//	Screenshots = true,
		//	Snapshots = true,
		//	Sources = true,
		//	Name = $"{nameof(CanAddHashtags)}.zip",
		//	Title = "Can Add Hashtags"
		//});

		await page.GotoAsync("/"); 

		await page.GetByPlaceholder("New Hashtag").FillAsync("dotnet");

		await page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

		string? firstHashtagContent = await page.Locator(".hashtags").First.TextContentAsync();
		Assert.Equal("dotnet", firstHashtagContent);

		//await page.Context.Tracing.StopAsync(new()
		//{
		//	Path = $"{nameof(CanAddHashtags)}.zip"
		//});


	}

}