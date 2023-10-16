using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Web;

namespace TagzApp.Providers.TwitchChat;

public class TwitchChatProvider : ISocialMediaProvider, IDisposable
{
	private bool _DisposedValue;
	private IChatClient? _Client;

	public string Id => "TWITCH";
	public string DisplayName => "TwitchChat";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(1);
	public string Description { get; init; } = "Twitch is where millions of people come together live every day to chat, interact, and make their own entertainment together.";

	private static readonly ConcurrentQueue<Content> _Contents = new();
	private static readonly CancellationTokenSource _CancellationTokenSource = new();
	private readonly TwitchChatConfiguration _Settings;
	private readonly ILogger<TwitchChatProvider> _Logger;
	private readonly TwitchProfileRepository _ProfileRepository;

	public TwitchChatProvider(TwitchChatConfiguration settings, ILogger<TwitchChatProvider> logger, IHttpClientFactory clientFactory)
	{
		_Settings = settings;
		_Logger = logger;
		_ProfileRepository = new TwitchProfileRepository(_Settings.ClientId, _Settings.ClientSecret, clientFactory.CreateClient("TwitchProfile"));

		if (!string.IsNullOrWhiteSpace(settings.Description))
		{
			Description = settings.Description;
		}
	}

	internal TwitchChatProvider(IOptions<TwitchChatConfiguration> settings, ILogger<TwitchChatProvider> logger, IChatClient chatClient)
	{
		_Settings = settings.Value;
		_Logger = logger;
		ListenForMessages(chatClient);
	}

	private async Task ListenForMessages(IChatClient chatClient = null)
	{

		var token = _CancellationTokenSource.Token;
		_Client = chatClient ?? new ChatClient(_Settings.ChannelName, _Settings.ChatBotName, _Settings.OAuthToken, _Logger);

		_Client.NewMessage += async (sender, args) =>
		{

			var profileUrl = await IdentifyProfilePic(args.UserName);

			_Contents.Enqueue(new Content
			{
				Provider = Id,
				ProviderId = args.MessageId,
				SourceUri = new Uri($"https://twitch.tv/{_Settings.ChannelName}"),
				Author = new Creator
				{
					ProfileUri = new Uri($"https://twitch.tv/{args.UserName}"),
					ProfileImageUri = new Uri(profileUrl),
					DisplayName = args.DisplayName,
					UserName = $"@{args.DisplayName}"
				},
				Text = HttpUtility.HtmlEncode(args.Message),
				Type = ContentType.Chat,
				Timestamp = args.Timestamp,
				Emotes = args.Emotes
			});
		};

		_Client.Init();

	}

	private async Task<string> IdentifyProfilePic(string userName)
	{
		return await _ProfileRepository.GetProfilePic(userName);
	}

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		var messages = _Contents.ToList();
		if (messages.Count() == 0) return Task.FromResult(Enumerable.Empty<Content>());

		var messageCount = messages.Count();
		for (var i = 0; i < messageCount; i++)
		{
			_Contents.TryDequeue(out _);
		}

		messages.ForEach(m => m.HashtagSought = tag.Text);

		return Task.FromResult(messages.AsEnumerable());

	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_Client.Dispose();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	~TwitchChatProvider()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public Task StartAsync()
	{
		ListenForMessages();
		return Task.CompletedTask;
	}
}
