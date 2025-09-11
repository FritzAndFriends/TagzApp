using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TagzApp.Providers.Kick;

public class ChatClient : IChatClient
{
	public const string LOGGER_CATEGORY = "Providers.Kick";
	
	private ClientWebSocket? _WebSocket;
	private CancellationTokenSource _Shutdown = new();
	private Task? _ReceiveMessagesTask;
	private readonly ILogger _Logger;

	public event EventHandler<NewMessageEventArgs>? NewMessage;

	public string ChannelName { get; private set; }
	public string ApiKey { get; private set; }

	public bool IsRunning { get; private set; }
	public bool IsConnected => _WebSocket?.State == WebSocketState.Open;

	public ChatClient(string channelName, string apiKey, ILogger logger)
	{
		ChannelName = channelName;
		ApiKey = apiKey;
		_Logger = logger;
	}

	public async void Init()
	{
		try
		{
			IsRunning = true;
			_WebSocket = new ClientWebSocket();

			// Kick.com WebSocket endpoint for chat
			var uri = new Uri($"wss://ws-us2.pusher.com/app/32cbd69e4b950bf97679?protocol=7&client=js&version=7.6.0&flash=false");
			
			await _WebSocket.ConnectAsync(uri, _Shutdown.Token);
			
			_Logger.LogInformation("Connected to Kick chat WebSocket");

			// Subscribe to the channel
			await SubscribeToChannel();

			// Start receiving messages
			_ReceiveMessagesTask = Task.Run(ReceiveMessages, _Shutdown.Token);
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Failed to initialize Kick chat client");
			IsRunning = false;
		}
	}

	private async Task SubscribeToChannel()
	{
		if (_WebSocket?.State != WebSocketState.Open) return;

		// Subscribe to the chat channel for the specified channel
		var subscribeMessage = new
		{
			@event = "pusher:subscribe",
			data = new { channel = $"chatrooms.{ChannelName}.v2" }
		};

		var messageJson = JsonSerializer.Serialize(subscribeMessage);
		var messageBytes = Encoding.UTF8.GetBytes(messageJson);
		
		await _WebSocket.SendAsync(
			new ArraySegment<byte>(messageBytes), 
			WebSocketMessageType.Text, 
			true, 
			_Shutdown.Token);

		_Logger.LogInformation("Subscribed to Kick chat channel: {ChannelName}", ChannelName);
	}

	private async Task ReceiveMessages()
	{
		var buffer = new byte[4096];
		
		while (_WebSocket?.State == WebSocketState.Open && !_Shutdown.Token.IsCancellationRequested)
		{
			try
			{
				var result = await _WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _Shutdown.Token);
				
				if (result.MessageType == WebSocketMessageType.Text)
				{
					var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
					await ProcessMessage(message);
				}
			}
			catch (Exception ex)
			{
				_Logger.LogError(ex, "Error receiving message from Kick chat");
				break;
			}
		}
	}

	private async Task ProcessMessage(string message)
	{
		try
		{
			using var jsonDoc = JsonDocument.Parse(message);
			var root = jsonDoc.RootElement;

			// Check if this is a chat message event
			if (root.TryGetProperty("event", out var eventElement) && 
			    eventElement.GetString() == "App\\Events\\ChatMessageEvent")
			{
				if (root.TryGetProperty("data", out var dataElement))
				{
					var dataString = dataElement.GetString();
					if (!string.IsNullOrEmpty(dataString))
					{
						var chatData = JsonSerializer.Deserialize<KickChatMessage>(dataString);
						if (chatData != null)
						{
							await ProcessChatMessage(chatData);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			_Logger.LogDebug(ex, "Failed to process Kick chat message: {Message}", message);
		}
	}

	private async Task ProcessChatMessage(KickChatMessage chatMessage)
	{
		var args = new NewMessageEventArgs
		{
			DisplayName = chatMessage.Sender?.Username ?? "Unknown",
			UserName = chatMessage.Sender?.Username ?? "Unknown",
			Message = chatMessage.Content ?? string.Empty,
			MessageId = chatMessage.Id?.ToString() ?? Guid.NewGuid().ToString(),
			Timestamp = chatMessage.CreatedAt,
			Badges = [],
			Emotes = []
		};

		NewMessage?.Invoke(this, args);
	}

	public void ListenToNewChannel(string channelName)
	{
		if (ChannelName != channelName)
		{
			ChannelName = channelName;
			
			// Reconnect to the new channel
			if (IsRunning)
			{
				Stop();
				Init();
			}
		}
	}

	public void Stop()
	{
		try
		{
			IsRunning = false;
			_Shutdown?.Cancel();
			_WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopping", CancellationToken.None);
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Error stopping Kick chat client");
		}
	}

	public void Dispose()
	{
		Stop();
		_WebSocket?.Dispose();
		_Shutdown?.Dispose();
	}

	// Data model for Kick chat messages
	private class KickChatMessage
	{
		public int? Id { get; set; }
		public string? Content { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public KickSender? Sender { get; set; }
	}

	private class KickSender
	{
		public string? Username { get; set; }
		public string? Slug { get; set; }
	}
}