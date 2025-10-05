using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace TagzApp.Providers.Discord;

/// <summary>
/// Service for managing Discord Gateway WebSocket connection
/// </summary>
public class DiscordGatewayService : IDisposable
{
	private readonly ILogger<DiscordGatewayService> _Logger;
	private readonly DiscordConfiguration _Config;
	private readonly ConcurrentQueue<DiscordMessage> _MessageQueue;
	
	private ClientWebSocket? _WebSocket;
	private CancellationTokenSource? _CancellationTokenSource;
	private Timer? _HeartbeatTimer;
	private int? _LastSequence;
	private bool _HeartbeatAcknowledged = true;
	private int _ReconnectAttempts = 0;
	private bool _IsDisposed = false;

	private const string GatewayUrl = "wss://gateway.discord.gg/?v=10&encoding=json";
	
	public event EventHandler<DiscordMessage>? MessageReceived;
	public event EventHandler<string>? ConnectionStatusChanged;

	public DiscordGatewayService(ILogger<DiscordGatewayService> logger, DiscordConfiguration config, ConcurrentQueue<DiscordMessage> messageQueue)
	{
		_Logger = logger;
		_Config = config;
		_MessageQueue = messageQueue;
	}

	/// <summary>
	/// Start the Gateway connection
	/// </summary>
	public async Task StartAsync(CancellationToken cancellationToken = default)
	{
		if (_IsDisposed) return;

		_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		
		try
		{
			await ConnectAsync(_CancellationTokenSource.Token);
			_ = Task.Run(() => ListenAsync(_CancellationTokenSource.Token), _CancellationTokenSource.Token);
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Failed to start Discord Gateway connection");
			ConnectionStatusChanged?.Invoke(this, $"Failed to connect: {ex.Message}");
			
			// Try to reconnect
			_ = Task.Run(() => ReconnectAsync(_CancellationTokenSource.Token), _CancellationTokenSource.Token);
		}
	}

	/// <summary>
	/// Stop the Gateway connection
	/// </summary>
	public async Task StopAsync()
	{
		if (_IsDisposed) return;

		_HeartbeatTimer?.Dispose();
		_CancellationTokenSource?.Cancel();

		if (_WebSocket?.State == WebSocketState.Open)
		{
			try
			{
				await _WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopping", CancellationToken.None);
			}
			catch (Exception ex)
			{
				_Logger.LogWarning(ex, "Error closing WebSocket connection");
			}
		}

		_WebSocket?.Dispose();
		_CancellationTokenSource?.Dispose();
	}

	private async Task ConnectAsync(CancellationToken cancellationToken)
	{
		_WebSocket?.Dispose();
		_WebSocket = new ClientWebSocket();
		
		_Logger.LogInformation("Connecting to Discord Gateway...");
		ConnectionStatusChanged?.Invoke(this, "Connecting...");

		await _WebSocket.ConnectAsync(new Uri(GatewayUrl), cancellationToken);
		
		_Logger.LogInformation("Connected to Discord Gateway");
		ConnectionStatusChanged?.Invoke(this, "Connected");
	}

	private async Task ListenAsync(CancellationToken cancellationToken)
	{
		var buffer = new byte[4096];
		var messageBuffer = new StringBuilder();

		try
		{
			while (_WebSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
			{
				messageBuffer.Clear();
				WebSocketReceiveResult result;

				do
				{
					result = await _WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
					if (result.MessageType == WebSocketMessageType.Text)
					{
						messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
					}
				} while (!result.EndOfMessage);

				if (result.MessageType == WebSocketMessageType.Text)
				{
					var message = messageBuffer.ToString();
					await HandleGatewayMessage(message, cancellationToken);
				}
				else if (result.MessageType == WebSocketMessageType.Close)
				{
					_Logger.LogWarning("WebSocket connection closed by Discord");
					ConnectionStatusChanged?.Invoke(this, "Disconnected");
					break;
				}
			}
		}
		catch (OperationCanceledException)
		{
			_Logger.LogInformation("Discord Gateway connection cancelled");
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Error in Discord Gateway listener");
			ConnectionStatusChanged?.Invoke(this, $"Error: {ex.Message}");
		}

		// Attempt reconnection if not disposed and not cancelled
		if (!_IsDisposed && !cancellationToken.IsCancellationRequested)
		{
			_ = Task.Run(() => ReconnectAsync(cancellationToken), cancellationToken);
		}
	}

	private async Task HandleGatewayMessage(string message, CancellationToken cancellationToken)
	{
		try
		{
			var payload = JsonSerializer.Deserialize<DiscordGatewayPayload>(message);
			if (payload == null) return;

			_LastSequence = payload.Sequence ?? _LastSequence;

			switch (payload.Op)
			{
				case DiscordGatewayOpcodes.Hello:
					await HandleHelloPayload(payload.Data, cancellationToken);
					break;

				case DiscordGatewayOpcodes.HeartbeatAck:
					_HeartbeatAcknowledged = true;
					break;

				case DiscordGatewayOpcodes.Dispatch:
					await HandleDispatchPayload(payload.Type, payload.Data);
					break;

				case DiscordGatewayOpcodes.Reconnect:
					_Logger.LogInformation("Discord requested reconnection");
					_ = Task.Run(() => ReconnectAsync(cancellationToken), cancellationToken);
					break;

				case DiscordGatewayOpcodes.InvalidSession:
					_Logger.LogWarning("Invalid session, reconnecting...");
					_ = Task.Run(() => ReconnectAsync(cancellationToken), cancellationToken);
					break;
			}
		}
		catch (JsonException ex)
		{
			_Logger.LogWarning(ex, "Failed to parse Gateway message: {Message}", message);
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Error handling Gateway message");
		}
	}

	private async Task HandleHelloPayload(object? data, CancellationToken cancellationToken)
	{
		if (data == null) return;

		var helloData = JsonSerializer.Deserialize<DiscordHelloPayload>(data.ToString()!);
		if (helloData == null) return;

		_Logger.LogInformation("Received Hello payload, heartbeat interval: {Interval}ms", helloData.HeartbeatInterval);

		// Start heartbeat timer
		_HeartbeatTimer?.Dispose();
		_HeartbeatTimer = new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(helloData.HeartbeatInterval));

		// Send identify payload
		await SendIdentifyPayload(cancellationToken);
	}

	private async Task SendIdentifyPayload(CancellationToken cancellationToken)
	{
		var identify = new DiscordGatewayPayload
		{
			Op = DiscordGatewayOpcodes.Identify,
			Data = new DiscordIdentifyPayload
			{
				Token = _Config.BotToken,
				Intents = DiscordGatewayIntents.GuildMessages | DiscordGatewayIntents.MessageContent,
				Properties = new DiscordConnectionProperties()
			}
		};

		await SendPayload(identify, cancellationToken);
		_Logger.LogInformation("Sent identify payload");
	}

	private async Task HandleDispatchPayload(string? eventType, object? data)
	{
		if (eventType == null || data == null) return;

		switch (eventType)
		{
			case "READY":
				_Logger.LogInformation("Discord Gateway ready");
				ConnectionStatusChanged?.Invoke(this, "Ready");
				_ReconnectAttempts = 0; // Reset reconnect attempts on successful connection
				break;

			case "MESSAGE_CREATE":
				var message = JsonSerializer.Deserialize<DiscordMessage>(data.ToString()!);
				if (message != null && ShouldProcessMessage(message))
				{
					_MessageQueue.Enqueue(message);
					MessageReceived?.Invoke(this, message);
				}
				break;
		}
	}

	private bool ShouldProcessMessage(DiscordMessage message)
	{
		// Check if message is from the configured channel
		if (message.ChannelId != _Config.ChannelId)
			return false;

		// Check if message is from the configured guild
		if (message.GuildId != _Config.GuildId)
			return false;

		// Filter bot messages if not allowed
		if (message.Author.Bot && !_Config.IncludeBotMessages)
			return false;

		// Filter system messages if not allowed
		if (message.Author.System && !_Config.IncludeSystemMessages)
			return false;

		// Filter message types
		if (!_Config.IncludeSystemMessages && message.Type != DiscordMessageType.Default && message.Type != DiscordMessageType.Reply)
			return false;

		// Check minimum message length
		if (message.Content.Length < _Config.MinMessageLength)
			return false;

		// Check blocked users
		var blockedUsers = _Config.GetBlockedUsersList();
		if (blockedUsers.Contains(message.Author.Id))
			return false;

		return true;
	}

	private void SendHeartbeat(object? state)
	{
		if (_WebSocket?.State != WebSocketState.Open || _IsDisposed)
			return;

		if (!_HeartbeatAcknowledged)
		{
			_Logger.LogWarning("Heartbeat not acknowledged, reconnecting...");
			_ = Task.Run(() => ReconnectAsync(CancellationToken.None));
			return;
		}

		_HeartbeatAcknowledged = false;

		var heartbeat = new DiscordGatewayPayload
		{
			Op = DiscordGatewayOpcodes.Heartbeat,
			Data = _LastSequence
		};

		_ = Task.Run(async () =>
		{
			try
			{
				await SendPayload(heartbeat, CancellationToken.None);
			}
			catch (Exception ex)
			{
				_Logger.LogWarning(ex, "Failed to send heartbeat");
			}
		});
	}

	private async Task SendPayload(DiscordGatewayPayload payload, CancellationToken cancellationToken)
	{
		if (_WebSocket?.State != WebSocketState.Open)
			return;

		var json = JsonSerializer.Serialize(payload);
		var bytes = Encoding.UTF8.GetBytes(json);

		await _WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
	}

	private async Task ReconnectAsync(CancellationToken cancellationToken)
	{
		if (_IsDisposed || _ReconnectAttempts >= _Config.MaxReconnectAttempts)
		{
			if (_ReconnectAttempts >= _Config.MaxReconnectAttempts)
			{
				_Logger.LogError("Max reconnection attempts reached, giving up");
				ConnectionStatusChanged?.Invoke(this, "Failed - Max reconnect attempts reached");
			}
			return;
		}

		_ReconnectAttempts++;
		var delay = TimeSpan.FromSeconds(Math.Pow(2, _ReconnectAttempts)); // Exponential backoff

		_Logger.LogInformation("Reconnecting to Discord Gateway (attempt {Attempt}/{Max}) in {Delay}s...", 
			_ReconnectAttempts, _Config.MaxReconnectAttempts, delay.TotalSeconds);

		ConnectionStatusChanged?.Invoke(this, $"Reconnecting (attempt {_ReconnectAttempts})...");

		try
		{
			await Task.Delay(delay, cancellationToken);
			await StopAsync();
			await ConnectAsync(cancellationToken);
			_ = Task.Run(() => ListenAsync(cancellationToken), cancellationToken);
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Reconnection attempt {Attempt} failed", _ReconnectAttempts);
			
			// Schedule another reconnect attempt
			_ = Task.Run(() => ReconnectAsync(cancellationToken), cancellationToken);
		}
	}

	public void Dispose()
	{
		if (_IsDisposed) return;
		
		_IsDisposed = true;
		
		_HeartbeatTimer?.Dispose();
		_CancellationTokenSource?.Cancel();
		_CancellationTokenSource?.Dispose();
		_WebSocket?.Dispose();
	}
}