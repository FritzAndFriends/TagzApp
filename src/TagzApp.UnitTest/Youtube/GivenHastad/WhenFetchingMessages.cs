// Ignore Spelling: Sut

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TagzApp.Providers.Youtube;
using TagzApp.Providers.Youtube.Configuration;

namespace TagzApp.UnitTest.Youtube.GivenHashtag;

public class YouTubeConfigurationFixture
{
	private readonly IConfigurationRoot _Config;

	public YouTubeConfigurationFixture()
	{
		var configBuilder = new ConfigurationBuilder();
		configBuilder.AddUserSecrets<YouTubeConfigurationFixture>();
		_Config = configBuilder.Build();

		_Config.GetSection(YoutubeConfiguration.AppSettingsSection).Bind(_YoutubeConfiguration);
	}

	public IConfigurationRoot Config => _Config;

	private YoutubeConfiguration _YoutubeConfiguration = new()
	{
		ApiKey = "",
		MaxResults = 1
	};

	public YoutubeConfiguration YoutubeConfiguration => _YoutubeConfiguration;
}

public class WhenFetchingMessages : IClassFixture<YouTubeConfigurationFixture>
{

	public readonly Hashtag Given = new()
	{
		Text = "dotnet"
	};
	private readonly YouTubeConfigurationFixture _YouTubeConfiguration;
	private YoutubeProvider _Sut;

	public WhenFetchingMessages(YouTubeConfigurationFixture youTubeConfiguration)
	{
		// Place your YouTube API Key for testing in the user secrets associated with this test project.

		var config = youTubeConfiguration.YoutubeConfiguration;
		_Sut = new YoutubeProvider(Options.Create(config));
		_YouTubeConfiguration = youTubeConfiguration;
	}

	[SkippableFact()]
	public async Task ShouldReceiveMessages()
	{
		Skip.If(string.IsNullOrEmpty(_YouTubeConfiguration.YoutubeConfiguration.ApiKey), "YouTube API Key required");

		// act
		var messages = await _Sut.GetContentForHashtag(Given, DateTimeOffset.UtcNow.AddHours(-1));

		// assert
		Assert.NotEmpty(messages);

	}

	[SkippableFact()]
	public async Task ShouldPopulateAuthorInformation()
	{
		Skip.If(string.IsNullOrEmpty(_YouTubeConfiguration.YoutubeConfiguration.ApiKey), "YouTube API Key required");

		// act
		var messages = await _Sut.GetContentForHashtag(Given, DateTimeOffset.UtcNow.AddHours(-1));

		// assert
		Assert.NotNull(messages.First().Author);
		Assert.NotNull(messages.First().Author.DisplayName);

	}

}

