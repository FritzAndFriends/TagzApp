// Ignore Spelling: Sut

using TagzApp.Providers.Youtube;

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
		_Sut = new YoutubeProvider();
  }

	[Fact]
	public async Task ShouldReceiveMessages()
	{

		// act
		var messages = await _Sut.GetContentForHashtag(Given, DateTimeOffset.UtcNow.AddHours(-1));

		// assert
		Assert.NotEmpty(messages);

	}

	[Fact]
	public async Task ShouldPopulateAuthorInformation()
	{

		// act
		var messages = await _Sut.GetContentForHashtag(Given, DateTimeOffset.UtcNow.AddHours(-1));

		// assert
		Assert.NotNull(messages.First().Author);
		Assert.NotNull(messages.First().Author.DisplayName);

	}

}

