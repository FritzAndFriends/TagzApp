using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Data;
using Xunit.Sdk;

namespace TagzApp.WebTest;

[TestCaseOrderer(ordererTypeName: "TagzApp.WebTest.PriorityOrderer",
  ordererAssemblyName: "TagzApp.WebTest")]
public class ModalWebTests : BaseFixture
{
  private readonly IPage _Page; //Shared page for all tests
  private readonly ITestOutputHelper _OutputHelper;

  public ModalWebTests(PlaywrightWebApplicationFactory webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper)
  {
    _OutputHelper = outputHelper;
    _Page = WebApp.CreateSingletonPlaywrightPageAsync(headless: false).GetAwaiter().GetResult();
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

    var contentModel = _Page.Locator("#contentModal").First;
    // Click CLose button
    await _Page.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();

    // Determine is modal is still visible
    var modalVisible = await _Page.Locator("#contentModal").IsVisibleAsync();
    Assert.False(modalVisible);

    //await page.Context.Tracing.StopAsync(new()
    //{
    //	Path = $"{nameof(CloseModal)}.zip"
    //});
  }

  public void Dispose()
  {

  }
}




