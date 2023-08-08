using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Data;
using Xunit.Sdk;

namespace TagzApp.WebTest;

[TestCaseOrderer(ordererTypeName: "TagzApp.WebTest.PriorityOrderer",
  ordererAssemblyName: "TagzApp.WebTest")]
public class ModalWebTests : BaseFixture
{
  private readonly IPage _Page; //Shared page for all tests

  public ModalWebTests(PlaywrightWebApplicationFactory webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper)
  {
    _Page = WebApp.CreateSingletonPlaywrightPageAsync(headless: true).GetAwaiter().GetResult();
  }

  // Creating PriorityOrder to help run tests in an order
  // Refs: https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit
  // TODO: There are test dependencies here - so state of one can affect the other.
  // Need to figure out how to skip the second test if the first fails.

  [Fact, TestPriority(1)]
  public async Task CanLaunchModal()
  {
    //await page.Context.Tracing.StartAsync(new()
    //{
    //	Screenshots = true,
    //	Snapshots = true,
    //	Sources = true,
    //	Name = $"{nameof(CanLaunchModal)}.zip",
    //	Title = "Can Launch Modal"
    //});

    await _Page.GotoAsync("/");
    await _Page.GetByPlaceholder("New Hashtag").FillAsync("dotnet");
    await _Page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();


    // Get first article element
    await _Page.Locator("article").WaitForAsync(new()
    {
      Timeout = 30000
    });
    // Get contents of first article element
    var article = _Page.Locator("article").First;
    var articleContents = await article.TextContentAsync();

    // Click first article element
    await article.ClickAsync();

    // Wait for modal to appear
    await _Page.Locator("#contentModal").WaitForAsync(new()
    {
      Timeout = 30000
    });

    // find modal
    var contentModel = _Page.Locator("#contentModal").First;
    // Determine Modal is visible
    var modalIsVisible = await contentModel.IsVisibleAsync();
    Assert.True(modalIsVisible);

    //Get contents of modal
    var modalContents = await contentModel.TextContentAsync();

    //await page.Context.Tracing.StopAsync(new()
    //{
    //	Path = $"{nameof(CanLaunchModal)}.zip"
    //});
  }

  [Fact, TestPriority(2)]
  public async Task CloseModal()
  {
    //await page.Context.Tracing.StartAsync(new()
    //{
    //	Screenshots = true,
    //	Snapshots = true,
    //	Sources = true,
    //	Name = $"{nameof(CloseModal)}.zip",
    //	Title = "Close Modal"
    //});

    // Modal should be in a state of being displayed
    // find modal
    Assert.True(await _Page.Locator("#contentModal").IsVisibleAsync());

    // Click Close button
    var closeButton = _Page.GetByRole(AriaRole.Button, new() { Name = "Close" }).First;
    await closeButton.ClickAsync(new LocatorClickOptions() { Delay = 500 });

    // Wait for modal to disappear (because the state of the dialog closure can cause the test to fail)
    int secondsToFail = 5;
    while (await _Page.Locator("#contentModal").IsVisibleAsync())
    {
      if (secondsToFail <= 0) { break; }
      await Task.Delay(1000);
      secondsToFail--;
    }

    // Determine is modal is still visible
    Assert.False(await _Page.Locator("#contentModal").IsVisibleAsync());

    //await page.Context.Tracing.StopAsync(new()
    //{
    //	Path = $"{nameof(CloseModal)}.zip"
    //});
  }

}




