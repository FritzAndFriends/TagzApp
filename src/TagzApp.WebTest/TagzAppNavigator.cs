namespace TagzApp.WebTest;

/// <summary>
/// A collection of extension methods that navigate the application.
/// </summary>
public static class TagzAppNavigator
{

	public static async Task<IPage> GotoHashtagSearchPage(this IPage page)
	{

		await page.GotoAsync("/");

		return page;

	}

	public static async Task<IPage> SearchForHashtag(this IPage page, string hashtag)
	{

		await page.GetByPlaceholder("New Hashtag").FillAsync(hashtag);

		await page.GetByRole(AriaRole.Button, new() { Name = "Add" }).ClickAsync();

		return page;

	}

	public static async Task<IPage> GotoWaterfallPage(this IPage page)
	{

		if (page.Url != "/waterfall") await page.GotoAsync("/waterfall");

		return page;

	}

	public static async Task<IPage> GotoOverlayPage(this IPage page)
	{

		if (page.Url != "/overlay") await page.GotoAsync("/overlay");

		return page;

	}

}
