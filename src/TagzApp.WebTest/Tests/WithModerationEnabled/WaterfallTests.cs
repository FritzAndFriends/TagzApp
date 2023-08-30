﻿using Xunit.Abstractions;

namespace TagzApp.WebTest.Tests.WithModerationEnabled;



public class WaterfallTests : IClassFixture<BaseModerationFixture>
{

	private IPage Page => _Webapp.Page; //Shared page for all tests
	private readonly BaseModerationFixture _Webapp;
	private readonly ITestOutputHelper _OutputHelper;

	public WaterfallTests(BaseModerationFixture webapp, ITestOutputHelper outputHelper)
	{
		_Webapp = webapp;
		_OutputHelper = outputHelper;
	}

	[Fact]
	public async Task NoUnapprovedContentShouldAppear()
	{
		var page = await _Webapp.CreatePlaywrightPageAsync();

		await using var trace = await page.TraceAsync("No Unapproved content should appear", true, true, true);

		await page
			.GotoHashtagSearchPage().Result
			.SearchForHashtag("dotnet").Result
			.GotoWaterfallPage();

		await Assert.ThrowsAsync<TimeoutException>(async () => 
			await page.Locator("article").First.WaitForAsync(new()
			{
				Timeout = 1000
			})
		);		

	}


}