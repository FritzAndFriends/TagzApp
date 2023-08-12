﻿using TagzApp.WebTest.Fixtures;

namespace TagzApp.WebTest;

public class OverlayTests : TestsBase
{
  public OverlayTests(PlaywrightWebApplicationFactory webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper)
  {
  }

  [Fact]
  public async Task ClickMessageOnWaterfallShowsOverlay()
  {

    const string TAG = "dotnet";

    // This test requires two unconnected contexts.
    // Here we setup 2 separate incognito contexts - The utility method means we don't have to worry about lifetimes and closing

    await using var context1 = await WebApp.CreatePlaywrightContextPageAsync();
    await using var context2 = await WebApp.CreatePlaywrightContextPageAsync();

    var mainPage = context1.Page;
    var overlayPage = context2.Page;

    // If required 2 browser instances can be setup instead

    //await using var browser1 = await WebApp.CreateCustomPlaywrightBrowserPageAsync();
    //await using var browser2 = await WebApp.CreateCustomPlaywrightBrowserPageAsync();
    //var mainPage = browser1.Page;
    //var overlayPage = browser2.Page;


    await mainPage.GotoAsync("/");

    await mainPage.GetByPlaceholder("New Hashtag").FillAsync(TAG);

    await mainPage.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

    await mainPage.Locator("article").WaitForAsync(new()
    {
      Timeout = 5000
    });

    await overlayPage.GotoAsync($"/overlay/{TAG}");

    var overlayContents = await overlayPage.Locator("#overlayDisplay").AllTextContentsAsync();
    Assert.Empty(string.Join('\n', overlayContents.Select(o => o.Trim()).ToArray()));

    // Click the first message on the waterfall display
    await mainPage.Locator("article").First.ClickAsync();

    // Inspect the overlay
    overlayContents = await overlayPage.Locator("#overlayDisplay").AllTextContentsAsync();
    Assert.NotEmpty(overlayContents);
  }
}
