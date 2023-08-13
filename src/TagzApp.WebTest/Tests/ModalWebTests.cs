﻿using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Sdk;

namespace TagzApp.WebTest;

// This fixture creates a new web application, new browser, and a single page for the lifetime of the fixture
// The fixture is in context for the duration of all the tests in a single class.
public class ModalFixture : PlaywrightPageFixture<Web.Program>
{
  public ModalFixture(IMessageSink output) : base(output) { }

  public bool SkipTest { get; set; }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    // ServicesExtensions.SocialMediaProviders = new List<IConfigureProvider> { new StartStubSocialMediaProvider() };

    builder.UseOnlyStubSocialMediaProvider();

    return base.CreateHost(builder);
  }
}

// We don't pull in the default TestsBase class as we have a different fixture in use for these tests.
[TestCaseOrderer(ordererTypeName: "TagzApp.WebTest.PriorityOrderer", ordererAssemblyName: "TagzApp.WebTest")]
public class ModalWebTests : IClassFixture<ModalFixture>
{
  private IPage Page => _Webapp.Page; //Shared page for all tests
  private readonly ModalFixture _Webapp;
  private readonly ITestOutputHelper _OutputHelper;

  public ModalWebTests(ModalFixture webapp, ITestOutputHelper outputHelper)
  {
    _Webapp = webapp;
    _OutputHelper = outputHelper;
  }

  // Creating PriorityOrder to help run tests in an order
  // Refs: https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit
  // There are test dependencies here - so state of one can affect the other.
  // We use the Xunit.SkippableFact package (https://github.com/AArnott/Xunit.SkippableFact) to allow skipping a test
  // At the start of each test, we check the fixture flag and skip if set, then set it.
  // At the end of the test (if we complete) we reset the flag.

  [Fact(),TestPriority(1)]
  public async Task CanLaunchModal()
  {
    Skip.If(_Webapp.SkipTest, "Previous test failed");
    _Webapp.SkipTest = true;

    // await using var trace = await page.TraceAsync("Can Launch Modal", true, true, true);

    await Page.GotoAsync("/");
    await Page.GetByPlaceholder("New Hashtag").FillAsync("dotnet");
    await Page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();


    // Get first article element
    await Page.Locator("article").WaitForAsync(new()
    {
      Timeout = 30000
    });
    // Get contents of first article element
    var article = Page.Locator("article").First;
    var articleContents = await article.TextContentAsync();

    // Click first article element
    await article.ClickAsync();

    // Wait for modal to appear
    await Page.Locator("#contentModal").WaitForAsync(new()
    {
      Timeout = 30000
    });

    // find modal
    var contentModel = Page.Locator("#contentModal").First;
    // Determine Modal is visible
    var modalIsVisible = await contentModel.IsVisibleAsync();
    Assert.True(modalIsVisible);

    //Get contents of modal
    var modalContents = await contentModel.TextContentAsync();

    // Assert.Equal(1, 2);  // To test if we skip the second test if the first one fails.

    _Webapp.SkipTest = false;
  }

  [SkippableFact(),TestPriority(2)]
  public async Task CloseModal()
  {
    Skip.If(_Webapp.SkipTest, "Previous test failed");
    _Webapp.SkipTest = true;
    
    // await using var trace = await page.TraceAsync("Close Modal", true, true, true);

    // Modal should be in a state of being displayed
    // find modal
    await Page.GotoAsync("/");
    var isModalVisible = await Page.Locator("#contentModal").IsVisibleAsync();
    if (!isModalVisible)
    {

      var article = Page.Locator("article").First;
      var articleContents = await article.TextContentAsync();

      // Click first article element
      await article.ClickAsync();

      // Wait for modal to appear
      await Page.Locator("#contentModal").WaitForAsync(new()
      {
        Timeout = 30000
      });

    }

    // Click Close button
    var closeButton = Page.GetByRole(AriaRole.Button, new() { Name = "Close" }).First;
    await closeButton.ClickAsync(new LocatorClickOptions() { Delay = 500 });

    // Wait for modal to disappear (because the state of the dialog closure can cause the test to fail)
    int secondsToFail = 5;
    await Page.Locator("#contentModal").WaitForAsync(new LocatorWaitForOptions
    {
      State = WaitForSelectorState.Hidden,
      Timeout = secondsToFail * 1000
    });

    // Determine is modal is still visible
    Assert.False(await Page.Locator("#contentModal").IsVisibleAsync());

    _Webapp.SkipTest = false;
  }

}




