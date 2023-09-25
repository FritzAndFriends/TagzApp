// See https://aka.ms/new-console-template for more information
using Microsoft.Playwright;

Console.WriteLine("Hello, World!");

var pw = await Playwright.CreateAsync();
await using var ff = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions() {
	Channel = "msedge",
	Headless = true,
	Args = new[] { "--mute-audio" },
});

var page = await ff.NewPageAsync();
await page.Context.Tracing.StartAsync(new TracingStartOptions()
{
Screenshots = true,
Snapshots = true,
Sources = true,
Name = "Test"
});

var response = await page.GotoAsync("https://www.youtube.com/watch?v=2NkUj2yyvX0");
var title = await page.Locator("#title h1").TextContentAsync(new LocatorTextContentOptions{ Timeout=(float)TimeSpan.FromSeconds(5).TotalMilliseconds});
title = title?.Trim();

Console.WriteLine($"Title: {title}");

var defaultClickTimeout = new LocatorClickOptions
{
	Timeout = (float)TimeSpan.FromSeconds(5).TotalMilliseconds
};

try
{

	// Stop video - we only want chat
	await page.GetByTitle("Pause", new PageGetByTitleOptions{ Exact=false}).ClickAsync(defaultClickTimeout);

	await page.FrameLocator("#chatframe").GetByLabel("Live Chat mode selection").ClickAsync(defaultClickTimeout);
	await page.FrameLocator("#chatframe").Locator("a").Filter(new() { HasText = "Live chat replay All messages are visible" }).ClickAsync(defaultClickTimeout);
} 
finally
{
	await page.Context.Tracing.StopAsync(new TracingStopOptions()
	{
		Path = "Test.zip"
	});
}
