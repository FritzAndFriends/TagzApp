// Ignore Spelling: Sut

using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.Metrics;
using TagzApp.Common.Telemetry;
using TagzApp.Providers.Mastodon;
using TagzApp.Providers.Mastodon.Configuration;
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
	private ProviderInstrumentation _Instrumentation;

	public WhenFetchingMessages()
	{
		var client = new HttpClient()
		{
			BaseAddress = new Uri("https://mas.to")
		};

		_HttpClientFactory = new StubHttpClientFactory(client);
		_Instrumentation = new ProviderInstrumentation(new StubMeterFactory());

		_Sut = new MastodonProvider(_HttpClientFactory, NullLogger<MastodonProvider>.Instance, new MastodonConfiguration(), _Instrumentation);
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

	internal class StubMeterFactory : IMeterFactory
	{
		public Meter Create(MeterOptions options)
		{
			return new Meter(options);
		}

		public void Dispose()
		{
		}
	}
}
