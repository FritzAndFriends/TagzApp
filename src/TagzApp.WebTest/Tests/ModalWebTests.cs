using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Sdk;

namespace TagzApp.WebTest;

// This fixture creates a new web application, new browser, and a single page for the lifetime of the fixture
// The fixture is in context for the duration of all the tests in a single class.
public class ModalFixture : PlaywrightPageFixture<Web.Program>
{
  public ModalFixture(IMessageSink output) : base(output) { }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    // ServicesExtensions.SocialMediaProviders = new List<IConfigureProvider> { new StartStubSocialMediaProvider() };

    builder.UseOnlyStubSocialMediaProvider();

    return base.CreateHost(builder);
  }
}

[TestCaseOrderer(ordererTypeName: "TagzApp.WebTest.PriorityOrderer", ordererAssemblyName: "TagzApp.WebTest")]
public class ModalWebTests : IClassFixture<ModalFixture>
{
  private IPage _Page => _Webapp.Page; //Shared page for all tests
  private readonly ModalFixture _Webapp;
  private readonly ITestOutputHelper _OutputHelper;

  public ModalWebTests(ModalFixture webapp, ITestOutputHelper outputHelper)
  {
    _Webapp = webapp;
    _OutputHelper = outputHelper;
  }

  // Creating PriorityOrder to help run tests in an order
  // Refs: https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit
  // TODO: There are test dependencies here - so state of one can affect the other.
  // Need to figure out how to skip the second test if the first fails.

  [Fact(Skip = "May be running too long on GitHub actions"), TestPriority(1)]
  public async Task CanLaunchModal()
  {
    // await using var trace = await page.TraceAsync("Can Launch Modal", true, true, true);

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
  }

  [Fact(Skip = "May be running too long on GitHub actions"), TestPriority(2)]
  public async Task CloseModal()
  {
    // await using var trace = await page.TraceAsync("Close Modal", true, true, true);

    // Modal should be in a state of being displayed
    // find modal
    await _Page.GotoAsync("/");
    var isModalVisible = await _Page.Locator("#contentModal").IsVisibleAsync();
    if (!isModalVisible)
    {

      var article = _Page.Locator("article").First;
      var articleContents = await article.TextContentAsync();

      // Click first article element
      await article.ClickAsync();

      // Wait for modal to appear
      await _Page.Locator("#contentModal").WaitForAsync(new()
      {
        Timeout = 30000
      });

    }

    // Click Close button
    var closeButton = _Page.GetByRole(AriaRole.Button, new() { Name = "Close" }).First;
    await closeButton.ClickAsync(new LocatorClickOptions() { Delay = 500 });

    // Wait for modal to disappear (because the state of the dialog closure can cause the test to fail)
    int secondsToFail = 5;
    await _Page.Locator("#contentModal").WaitForAsync(new LocatorWaitForOptions
    {
      State = WaitForSelectorState.Hidden,
      Timeout = secondsToFail * 1000
    });

    // Determine is modal is still visible
    Assert.False(await _Page.Locator("#contentModal").IsVisibleAsync());
  }

}




