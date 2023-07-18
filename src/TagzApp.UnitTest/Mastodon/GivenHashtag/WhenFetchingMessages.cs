// Ignore Spelling: Sut

using TagzApp.Providers.Mastodon;

namespace TagzApp.UnitTest.Mastodon.GivenHashtag;

public class WhenFetchingMessages
{

	public readonly Hashtag Given = new Hashtag()
	{
		Text = "dotnet"
	};

	private MastodonProvider _Sut;

  public WhenFetchingMessages()
  {
		var client = new HttpClient();
		StartMastodon.ConfigureHttpClient(client);
		_Sut = new MastodonProvider(client);
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

