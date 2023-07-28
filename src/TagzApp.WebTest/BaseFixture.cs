namespace TagzApp.WebTest;

public abstract class BaseFixture : IClassFixture<PlaywrightWebApplicationFactory>
{

	protected readonly PlaywrightWebApplicationFactory WebApp;
	protected readonly ITestOutputHelper OutputHelper;

	protected BaseFixture(PlaywrightWebApplicationFactory webapp, ITestOutputHelper outputHelper)
	{

		WebApp = webapp;
		OutputHelper = outputHelper;

	}

}
