using SUT = TagzApp.Providers.TwitchChat.ChatClient;

namespace TagzApp.UnitTest.TwitchChat.ChatClient;


public class GivenMessageWithEmotes
{

	const string RawMessageWithEmotes = """
		@badge-info=subscriber/65;badges=vip/1,subscriber/48,glitchcon2020/1;client-nonce=1b97cd0780e2dc2b5f61d800a6f7d278;color=#1E90FF;display-name=csharpfritz;emotes=302930318:8-18/140102:97-107;first-msg=0;flags=;id=c986dadb-d9d5-4cb2-b8bb-953792fb6a41;mod=0;returning-chatter=0;room-id=63208102;subscriber=1;tmi-sent-ts=1695218877167;turbo=0;user-id=96909659;user-type=;vip=1 :csharpfritz!csharpfritz@csharpfritz.tmi.twitch.tv PRIVMSG #fiercekittenz :Lasers! csharpGuess  and challenge coins?  I've wanted to pull the trigger on those for a while! kittenzHypu
		""";

	[Fact]
	public void ShouldReturnMultipleEmotes()
	{

		var outEmotes = SUT.IdentifyEmotes(RawMessageWithEmotes);

		Assert.Equal(2, outEmotes.Count);

	}

	[Fact]
	public void ShouldReturnCorrectEmotes()
	{

		var outEmotes = SUT.IdentifyEmotes(RawMessageWithEmotes);
		var testEmote = outEmotes.First();

		Assert.Contains("302930318", testEmote.ImageUrl);
		Assert.Equal(8, testEmote.Pos);
		Assert.Equal(11, testEmote.Length);

	}

}
