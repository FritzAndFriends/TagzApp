using Xunit;
using TagzApp.Providers.YouTubeChat;
using System.Net.Http;
using System.Reflection;

namespace TagzApp.UnitTest.YouTubeChat;

public class YouTubeEmoteTranslatorTests
{
	[Fact]
	public void TryIdentifyEmotes_FindsUpsideDownEmote()
	{
		// Arrange
		var sample = "Atilla says hello! :upside_down_face:";

		// Build a minimal JSON payload that LoadEmotes can parse containing the desired shortcut
		var json = "[{" +
			"\"emojiId\": \"u\"," +
			"\"image\": { \"thumbnails\": [ { \"url\": \"https://example.com/upside.svg\" } ] }," +
			"\"searchTerms\": [\"upside\",\"down\"]," +
			"\"shortcuts\": [\":upside_down_face:\"] } ]";

		var httpClient = new HttpClient(new TestHttpMessageHandler(json));
		YouTubeEmoteTranslator.LoadEmotes(httpClient, 0).GetAwaiter().GetResult();

		// Act
		var result = YouTubeEmoteTranslator.TryIdentifyEmotes(sample, out var emotes);

		// Assert
		Assert.True(result);
		Assert.Single(emotes);
		var emote = emotes[0];
		Assert.Equal(19, emote.Pos); // position of ":upside_down:" in sample (zero-based)
		Assert.Equal(18, emote.Length); // length of ":upside_down_face:"
		Assert.Equal("https://example.com/upside.svg", emote.ImageUrl);
	}

	// Minimal HttpMessageHandler that returns the provided JSON for any request.
	private class TestHttpMessageHandler : HttpMessageHandler
	{
		private readonly string _json;
		public TestHttpMessageHandler(string json) => _json = json;
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
			{
				Content = new StringContent(_json, System.Text.Encoding.UTF8, "application/json")
			};
			return Task.FromResult(resp);
		}
	}
}
