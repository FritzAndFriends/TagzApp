namespace TagzApp.WebTest;

public class MyFirstTests : BaseFixture
{
	private readonly PlaywrightWebApplicationFactory _WebApp;
	private readonly ITestOutputHelper _OutputHelper;

	public MyFirstTests(PlaywrightWebApplicationFactory webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper) { }

	[Fact]
	public async Task CanAddHashtags()
	{

		var page = await WebApp.CreatePlaywrightPageAsync();

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

	[Fact]
	public async Task LoadContentFromSocialMediaProvider()
	{

		var page = await WebApp.CreatePlaywrightPageAsync();

		await page.Context.Tracing.StartAsync(new()
		{
			Screenshots = true,
			Snapshots = true,
			Sources = true, 
			Name = $"{nameof(LoadContentFromSocialMediaProvider)}.zip",
			Title = "Load Content From Social Media Provider"
		});

		await page.GotoAsync("/");

		await page.GetByPlaceholder("New Hashtag").FillAsync("dotnet");

		await page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

		string? firstHashtagContent = await page.Locator(".hashtags").First.TextContentAsync();

		try
		{
			await page.Locator("article").WaitForAsync(new()
			{
				Timeout = 5000
			});
		}
		finally
		{

			await page.Context.Tracing.StopAsync(new()
			{
				Path = $"{nameof(LoadContentFromSocialMediaProvider)}.zip"
			});

		}


	}

	[Fact]
	public async Task LatestContentShouldBeFirst()
	{

		var page = await WebApp.CreatePlaywrightPageAsync();
		await page.GotoAsync("/");
		await page.GetByPlaceholder("New Hashtag").FillAsync("dotnet");
		await page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

		// Stub generates 10 articles.
    await page.WaitForSelectorAsync("article:nth-child(10)", new() { Timeout = 10000 });

		DateTime? lastArticleTime = null;
		foreach (var article in await page.Locator("article").AllAsync())
		{

      var articleTime = await article.Locator(".time").TextContentAsync();
      Assert.NotNull(articleTime);
			Assert.True(DateTime.TryParse(articleTime, out var currentArticleTime));

      if (lastArticleTime != null)
			{
        Assert.True(lastArticleTime > currentArticleTime);
      }

      lastArticleTime = currentArticleTime;

    }

  }

}