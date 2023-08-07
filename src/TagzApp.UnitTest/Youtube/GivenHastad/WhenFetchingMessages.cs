// Ignore Spelling: Sut

using Microsoft.Extensions.Options;
using TagzApp.Providers.Youtube;
using TagzApp.Providers.Youtube.Configuration;

namespace TagzApp.UnitTest.Youtube.GivenHashtag;

public class WhenFetchingMessages
{

	public readonly Hashtag Given = new Hashtag()
	{
		Text = "dotnet"
	};

	private YoutubeProvider _Sut;

	public WhenFetchingMessages()
	{
		_Sut = new YoutubeProvider(Options.Create(new YoutubeConfiguration() { ApiKey = "foo" }));
	}

	[Fact(Skip = "YouTube API Key required")]
	public async Task ShouldReceiveMessages()
	{

		// act
		var messages = await _Sut.GetContentForHashtag(Given, DateTimeOffset.UtcNow.AddHours(-1));

		// assert
		Assert.NotEmpty(messages);

	}

	[Fact(Skip = "YouTube API Key required")]
	public async Task ShouldPopulateAuthorInformation()
	{

		// act
		var messages = await _Sut.GetContentForHashtag(Given, DateTimeOffset.UtcNow.AddHours(-1));

		// assert
		Assert.NotNull(messages.First().Author);
		Assert.NotNull(messages.First().Author.DisplayName);

	}

}

