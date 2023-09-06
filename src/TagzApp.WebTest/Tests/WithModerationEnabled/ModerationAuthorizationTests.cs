using System.Net;
using TagzApp.Web;
using TagzApp.WebTest.Fixtures;

namespace TagzApp.WebTest.Tests.WithModerationEnabled;

public class ModerationAuthorizationTests : TestsBase
{
	public ModerationAuthorizationTests(PlaywrightFixture webapp, ITestOutputHelper outputHelper) : base(webapp, outputHelper)
	{
	}

	[Fact(Skip = "Forcing deploy")]
	public async Task AnonymousCantModerate()
	{
		await using var context = await WebApp.CreatePlaywrightContextPageAsync();
		var page = context.Page;

		// N.B. These tests work differently if the browser is not headless!
		IResponse? response = await page.GotoAsync("/Moderation");

		Assert.NotNull(response);
		Assert.Equal((int)HttpStatusCode.MisdirectedRequest, response.Status);

	}

	[Fact(Skip = "Don't run tests that require the UI on test servers")]
	public async Task AnonymousCantModerationUI()
	{
		await using var context = await WebApp.CreateCustomPlaywrightBrowserPageAsync(browserOptions: browser => browser.Headless = false);
		var page = context.Page;

		// N.B. These tests work differently if the browser is not headless!
		await Assert.ThrowsAsync<PlaywrightException>(async () => await page.GotoAsync("/Moderation"));

		Assert.NotEqual(WebApp.Uri + "/Moderation", page.Url);
		Assert.Equal("about:blank", page.Url);
	}

	[Fact(Skip = "Forcing deploy")]
	public async Task NonModeratorCantModerate()
	{
		await using var context = await WebApp.CreateAuthorisedPlaywrightBrowserPageAsync("user");
		var page = context.Page;

		IResponse? response = await page.GotoAsync("/Moderation");

		Assert.NotNull(response);
		Assert.Equal((int)HttpStatusCode.Forbidden, response.Status);
	}

	[Fact(Skip = "Forcing deploy")]
	public async Task ModeratorCanModerate()
	{
		await using var context = await WebApp.CreateAuthorisedPlaywrightBrowserPageAsync(Security.Role.Moderator);
		var page = context.Page;

		await page.GotoAsync("/Moderation");

		Assert.Equal(WebApp.Uri + "/Moderation", page.Url);

	}

	[Fact(Skip = "Forcing deploy")]
	public async Task AdminCanModerate()
	{
		await using var context = await WebApp.CreateAuthorisedPlaywrightBrowserPageAsync(Security.Role.Admin);
		var page = context.Page;

		await page.GotoAsync("/Moderation");

		Assert.Equal(WebApp.Uri + "/Moderation", page.Url);

	}

}

