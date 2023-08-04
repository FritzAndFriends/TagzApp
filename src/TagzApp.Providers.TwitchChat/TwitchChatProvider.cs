namespace TagzApp.Providers.TwitchChat;

public class TwitchChatProvider : ISocialMediaProvider, IDisposable
{
	private bool _DisposedValue;

	public string Id => "TWITCHCHAT";
	public string DisplayName => "TwitchChat";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(5);

	private static readonly Queue<Content> _Contents = new();
	private static readonly CancellationTokenSource _CancellationTokenSource = new();
	private Task _ContentRetrievalTask = null;

  public TwitchChatProvider()
  {
    
		_ContentRetrievalTask = Task.Run(ListenForMessages, _CancellationTokenSource.Token);

  }

	private async void ListenForMessages()
	{
		
		var token = _CancellationTokenSource.Token;
		while (!token.IsCancellationRequested)
		{

			await Task.Delay(100, token);

		}

	}

	/// <summary>
	/// The Twitch channel to monitor
	/// </summary>
	public string Channel { get; set; } = string.Empty;

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		throw new NotImplementedException();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~TwitchChatProvider()
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