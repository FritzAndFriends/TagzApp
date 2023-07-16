using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TagzApp.WebTest;


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

		await Page.GotoAsync("http://localhost:5038"); // AssemblyStart.App.Urls.First());

		await Page.GetByPlaceholder("New Hashtag").FillAsync("dotnet");

		await Page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

		await Expect(Page.Locator(".hashtags").First).ToHaveTextAsync("dotnet");

		await Page.Context.Tracing.StopAsync(new () {
			Path= $"{nameof(CanAddHashtags)}.zip"
		});


	}

}