using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TagzApp.WebTest
{

	[Parallelizable(ParallelScope.Self)]
	[TestFixture]
	public class MyFirstTests : PageTest
	{

		[Test]
		public async Task CanAddHashtags()
		{

			await Page.Context.Tracing.StartAsync(new()
			{
				Screenshots = true,
				Snapshots = true,
				Sources = true,
				Name = $"{nameof(CanAddHashtags)}.zip",
				Title = "Can Add Hashtags"
			});

			await Page.GotoAsync("https://localhost:7007/");

			var newHashtagLocator = Page.GetByPlaceholder("New Hashtag");
			await newHashtagLocator.FillAsync("dotnet");

			var addButtonLocator = Page.GetByRole(AriaRole.Button, new() { Name = "Add" });

			await Page.Context.Tracing.StopAsync(new () {
				Path= $"{nameof(CanAddHashtags)}.zip"
			});


		}

		[Test]
		public async Task HomepageHasPlaywrightInTitleAndGetStartedLinkLinkingtoTheIntroPage()
		{

			await using var browser = await Playwright.Chromium.LaunchAsync();
			await using var context = await browser.NewContextAsync();

			// Start tracing before creating / navigating a page.
			await context.Tracing.StartAsync(new()
			{
				Screenshots = true,
				Snapshots = true,
				Sources = true
			});

			var page = await context.NewPageAsync();

			await page.GotoAsync("https://playwright.dev");

			// Expect a title "to contain" a substring.
			await Expect(page).ToHaveTitleAsync(new Regex("Playwright"));

			// create a locator
			var getStarted = page.GetByRole(AriaRole.Link, new() { Name = "Get started" });

			// Expect an attribute "to be strictly equal" to the value.
			await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

			// Click the get started link.
			await getStarted.ClickAsync();

			// Expects the URL to contain intro.
			await Expect(page).ToHaveURLAsync(new Regex(".*intro"));

			await context.Tracing.StopAsync(new()
			{
				Path = "trace.zip"
			});

		}


	}
}
