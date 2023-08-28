// Ignore Spelling: Sut

using Microsoft.Extensions.Logging.Abstractions;
using TagzApp.Common.Models;
using TagzApp.Providers.Mastodon;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace TagzApp.UnitTest.Mastodon.GivenHashtag;

public class WhenFetchingMessages
{
	public readonly Hashtag Given = new()
	{
		Text = "dotnet"
	};

	private MastodonProvider _Sut;
	private IHttpClientFactory _HttpClientFactory;

	public WhenFetchingMessages()
	{
		var client = new HttpClient()
		{
			BaseAddress = new Uri("https://mas.to")
		};

		_HttpClientFactory = new StubHttpClientFactory(client);

		_Sut = new MastodonProvider(_HttpClientFactory, NullLogger<MastodonProvider>.Instance);
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

	internal class StubHttpClientFactory : IHttpClientFactory
	{
		private readonly HttpClient _Client;

		public StubHttpClientFactory(HttpClient client)
		{
			_Client = client;
		}

		public HttpClient CreateClient(string name)
		{
			return _Client;
		}
	}
}