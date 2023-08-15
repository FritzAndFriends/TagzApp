using Microsoft.Playwright;
using TagzApp.WebTest.Fixtures;

namespace TagzApp.WebTest;

public class MyFirstTests : TestsBase
{
  public MyFirstTests(PlaywrightFixture webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper) { }

  [Fact]
  public async Task CanAddHashtags()
  {
    var page = await WebApp.CreatePlaywrightPageAsync();

    // await using var trace = await page.TraceAsync("Can Add Hashtags", true, true, true);

    await page
      .GotoHashtagSearchPage().Result
      .SearchForHashtag("dotnet");

    string? firstHashtagContent = await page.Locator(".hashtags").First.TextContentAsync();
    Assert.Equal("dotnet", firstHashtagContent);
  }

  [Fact]
  public async Task LoadContentFromSocialMediaProvider()
  {

    var page = await WebApp.CreatePlaywrightPageAsync();

    await using var trace = await page.TraceAsync("Load Content From Social Media Provider", true, true, true);

    await page
      .GotoHashtagSearchPage().Result
      .SearchForHashtag("dotnet").Result
      .GotoWaterfallPage();

    // string? firstHashtagContent = await page.Locator(".hashtags").First.TextContentAsync();

    await page.Locator("article").WaitForAsync(new()
    {
      Timeout = 5000
    });
  }

  [Fact]
  public async Task ContentShouldBeInDescendingOrder()
  {

    var page = await WebApp.CreatePlaywrightPageAsync();

    await using var trace = await page.TraceAsync("Content Should Be In Descending Order", true, true, true);

    await page
      .GotoHashtagSearchPage().Result
      .SearchForHashtag("dotnet").Result
      .GotoWaterfallPage();

    // Stub generates 10 articles.
    await page.WaitForSelectorAsync("article:nth-child(10)", new() { Timeout = 15000 });

    var timeElements = await page.Locator("article").AllAsync();

    var parsedTimes = await Task.WhenAll(timeElements.Select(async te =>
    {
      var dataTimeAttribute = await te.GetAttributeAsync("data-timestamp");
      bool success = DateTime.TryParse(dataTimeAttribute, out DateTime time);
      return (success, time);
    }));

    Assert.True(parsedTimes.All(pt => pt.success));
    Assert.Equal(parsedTimes.Select(pt => pt.time).OrderByDescending(pt => pt), parsedTimes.Select(pt => pt.time));
  }
}