using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatProvider : ISocialMediaProvider, IDisposable
{
	private readonly YouTubeChatConfiguration _ChatConfig;

	public string Id => "YOUTUBE-CHAT";
	public string DisplayName => "YouTube Chat";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(10);

	public string NewestId { get; set; }

	private CancellationTokenSource _TokenSource = new();
	private string _LiveChatId = string.Empty;
	private YouTubeService _Service;
	private bool _DisposedValue;
	private Stream _ChatStream;

	public YouTubeChatProvider(IOptions<YouTubeChatConfiguration> chatConfig)
	{
		_ChatConfig = chatConfig.Value;
	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		var content = new List<Content>();

		// liveChatListRequest.

		return content;


	}

	public async Task StartAsync()
	{

		var credential = new ServiceAccountCredential(
			new ServiceAccountCredential.Initializer("tagzapp-provider@tagzapp-399616.iam.gserviceaccount.com")
			{
				Scopes = new[] { YouTubeService.Scope.YoutubeReadonly }
			}.FromPrivateKey("-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDDWTnwhr1E0esS\niAMFiQYfHgQCCmqem5LCQgaBrp9N6ZcQfjJDPPh7JxSaIcOGbd+HlK0XTQCLnAXO\nSSZ39LswpL/t27StIfVSPRVVuPir2wC8yyjtpwZ8xcBnMjSkrYbg28COQ1MrDJku\n5rB8Yf1nLrSMdJGwTO7dBJ0DotopuPW7oY/u2y65jO/3V0hyW2fjNeQbLqKSu36b\n5vRnOaa1muKoUos6XuZHnpWIPF2qAaYlzR0OJoKlqWrqbtiy/iqto4Qdprz4o0UQ\nZjU1SgR83Rln1e0X1S1ZwuA6mcW4eDgcVEBlbUHh+Ms7X+GJp7JUrJ1wKFyFBa65\n2h3ZaTcJAgMBAAECggEAYH+VJYIJwRNJYjAZ/gQAKCL1q+RlYtBLIPboq+sM+rnn\nS9hrD9fwjzVw8eq34ZIpF5qUHqyFFnIOVCbCgAM+7PqVbPRZPiVGQEe4YE4tWQeo\nR5q8LxmRFNXDA7dDVg36UN19M0mCrgNdMFP43pixSuVBfxieV07JuBhtT7yEtJKy\nSBY8gWpCbHfffLGnMOM1lwcwJO/1SsVVhUnihw9bQzhyJ2HkDHHFp95A/GqsZrmR\nHkhBr7UTEWPuBbdxvoOBub+C+1f6XUVKYFMWPJldgCq2flgM5v0LdND1ComE2/4f\nNrC9Csin3rU8Z7kjkqQd9fxvmb3FzjZF0Y7yWJwjOQKBgQDtpS/uiiy85Zg+eKCt\noAmo+Oge90D6beOTuBqovgKHAJ80mHn8/y3jstyPlzVHy/yuiKR2AKQMDP1U70se\ndDY2ai/vFBzq7wy0UkCsUN0gEVsdtzQDHcRZBZintxtPH2r594zic3tLGhKwjPAF\nPbdMCN6RRTg3witFLVfawtXn1wKBgQDSb7tvY3UlyGyar14WljPmFmf3bnrujBh9\nEH8SUtV2ctSjIo4OC/P2eDJJzI+ELsmlVC1BSFH3xVujh4z/W5oq2PVO4+gQuAg3\nR4mDhIJykgAKY8Q5F4OUqHTahzTE90E/0Eto7LJp22PjoxXUlKDD9+8WswA3I3OZ\nAvFJBbp8HwKBgD1daWokXfcNJmoDRiJvb+8lDvNoD2xbUefEI4YtQoPP4kx3jxCI\naDYi6pddiVGX2BDNkgIyminOdOAoxnH/ujwE4YnP3MPBpLsvfinA28i7EpcyxoiD\neD8wlcHBI5kj2MDhbozPGyhCfE8Apb4EuL82jxpeEG09g2Til4wSwZ+TAoGAD7lb\nlxImccFNJC3QaP2mOR4ZfKqbsvFy7v1pkVPxMV2ZN+tgE/qeqx8GGu+XFbhtRtZI\nX2VpAouTsl9xkK8mkOcPRWrQL1eg/Yhx5QrkuGziZeRYiC+SnGwN9zo9Hi6fiIYm\n7FsrZa+IAj1wZycH8Dy5d8e+T2BtxdQrdVphOd8CgYEAw6gHMPMCqrwPUdMCG5Cv\n+/xYiCHu1WB/Y+H/orz9mf9RU/APF76gtHO866ZLxP3Kv+IJWCbiStAoOMhlh4uE\nRWES8Yo1DwACKzeufNLnIkjkBaK4nt4kR2wwWB5pSUK+0IcBEhiW6XbEPW/hMPiP\nvyYbvtk5xTPgt3LGAH8jA64=\n-----END PRIVATE KEY-----\n")
	);

		var initializer = new BaseClientService.Initializer()
		{
			ApplicationName = "TagzApp",
			HttpClientInitializer = credential
		};

		_Service = new YouTubeService(initializer);
		var list = new LiveBroadcastsResource(_Service);
		var listRequest = list.List(new Google.Apis.Util.Repeatable<string>(new[] { "snippet" }));
		listRequest.Id = _ChatConfig.BroadcastId;

		try
		{
			var response = listRequest.Execute();

			if (response is not null)
			{
				_LiveChatId = response.Items.FirstOrDefault()?.Snippet?.LiveChatId ?? string.Empty;
			}

		}
		catch (Exception ex)
		{

			Console.WriteLine(ex);

		}

		if (string.IsNullOrEmpty(_LiveChatId)) throw new Exception("LiveChatId is not set.");
		var liveChatListRequest = new LiveChatMessagesResource.ListRequest(_Service, _LiveChatId, new(new[] { "id", "snippet", "authorDetails" }));
		_ChatStream = await liveChatListRequest.ExecuteAsStreamAsync(_TokenSource.Token);

	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_Service.Dispose();
				_TokenSource.Cancel();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~YouTubeChatProvider()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
