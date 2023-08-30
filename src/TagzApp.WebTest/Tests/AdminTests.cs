using System.Net;
using TagzApp.Web;
using TagzApp.WebTest.Fixtures;

namespace TagzApp.WebTest.Tests;

public class AdminTests : TestsBase
{
	public AdminTests(PlaywrightFixture webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper)
	{
	}

	[Fact]
	public async Task AnonymousCantAdmin()
	{
		await using var context = await WebApp.CreatePlaywrightContextPageAsync();
		var page = context.Page;

		// N.B. These tests work differently if the browser is not headless!
		IResponse? response = await page.GotoAsync("/Admin");

		Assert.NotNull(response);
		Assert.Equal((int)HttpStatusCode.MisdirectedRequest, response.Status);

	}

	[Fact(Skip="Don't run tests that require the UI on test servers")]
	public async Task AnonymousCantAdminUI()
	{
		await using var context = await WebApp.CreateCustomPlaywrightBrowserPageAsync(browserOptions: browser=>browser.Headless=false);
		var page = context.Page;

		// N.B. These tests work differently if the browser is not headless!
		await Assert.ThrowsAsync<PlaywrightException>(async ()=> await page.GotoAsync("/Admin"));

		Assert.NotEqual(WebApp.Uri + "/Admin", page.Url);
		Assert.Equal("about:blank", page.Url);
	}

	[Fact]
	public async Task NonAdminCantAdmin()
	{
		await using var context = await WebApp.CreateAuthorisedPlaywrightBrowserPageAsync("user");
		var page = context.Page;

		IResponse? response= await page.GotoAsync("/Admin");

		Assert.NotNull(response);
		Assert.Equal((int)HttpStatusCode.Forbidden, response.Status);
	}

	[Fact]
	public async Task AdminCanAdmin()
	{
		await using var context = await WebApp.CreateAuthorisedPlaywrightBrowserPageAsync(Security.Role.Admin);
		var page = context.Page;

		await page.GotoAsync("/Admin");

		Assert.Equal(WebApp.Uri + "/Admin", page.Url);

	}

}

