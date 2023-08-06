using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace TagzApp.Providers.TwitchChat;

public class TwitchChatProvider : ISocialMediaProvider, IDisposable
{
	private bool _DisposedValue;
	private IChatClient? _Client;

	public string Id => "TWITCH";
	public string DisplayName => "TwitchChat";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(5);

	private static readonly ConcurrentQueue<Content> _Contents = new();
	private static readonly CancellationTokenSource _CancellationTokenSource = new();
	private readonly TwitchChatConfiguration _Settings;
	private readonly ILogger<TwitchChatProvider> _Logger;

	public TwitchChatProvider(IOptions<TwitchChatConfiguration> settings, ILogger<TwitchChatProvider> logger)
	{

		_Settings = settings.Value;
		_Logger = logger;
		ListenForMessages();
	}

	internal TwitchChatProvider(IOptions<TwitchChatConfiguration> settings, ILogger<TwitchChatProvider> logger, IChatClient chatClient )
	{

		_Settings = settings.Value;
		_Logger = logger;
		ListenForMessages(chatClient);
	}

	/// <summary>
	/// The Twitch channel to monitor
	/// </summary>
	public string Channel { get; set; } = "csharpfritz";

	private async void ListenForMessages(IChatClient chatClient = null)
	{
		
		var token = _CancellationTokenSource.Token;
		_Client = chatClient ?? new ChatClient(Channel, _Settings.ChatBotName, _Settings.OAuthToken, _Logger);
		
		_Client.NewMessage += (sender, args) =>
		{
			_Contents.Enqueue(new Content
			{
				Provider = this.Id,
				ProviderId = args.MessageId,
				SourceUri = new Uri($"https://twitch.tv/{Channel}"),
				Author = new Creator {
					ProfileUri = new Uri($"https://twitch.tv/{args.UserName}"),
					ProfileImageUri = new Uri("https://twitch.tv"),		// TODO: Fetch and cache a collection of profile image URLs
					DisplayName = args.DisplayName
				},
				Text = args.Message,
				Type = ContentType.Chat,
				Timestamp = args.Timestamp
			});
		};
		
		_Client.Init();

	}

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		
		var messages = _Contents.Where(c => c.Timestamp >= since).ToList();
		if (messages.Count() == 0) return Task.FromResult(Enumerable.Empty<Content>());

		var messageCount = messages.Count();
		for (var i=0; i<messageCount; i++)
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
}