﻿using System.Text;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace TagzApp.Providers.TwitchChat;


public class ChatClient : IChatClient
{

	public const string LOGGER_CATEGORY = "Providers.TwitchChat";
	private TcpClient _TcpClient;
	private StreamReader inputStream;
	private StreamWriter outputStream;
	private int _Retries;
	private Task _ReceiveMassagesTask;
	private MemoryStream _ReceiveStream = new();

	internal static readonly Regex reUserName = new(@"!([^@]+)@");
	internal static readonly Regex reBadges = new(@"badges=([^;]*)");
	internal static readonly Regex reDisplayName = new(@"display-name=([^;]*)");
	internal static readonly Regex reTimestamp = new(@"tmi-sent-ts=(\d+)");
	internal static readonly Regex reMessageId = new(@"id=([^;]*)");
	internal static readonly Regex reEmotes = new("emotes=([^;]+;)");

	internal static Regex reChatMessage;
	internal static Regex reWhisperMessage;

	public event EventHandler<NewMessageEventArgs> NewMessage;

	private DateTime _NextReset;
	private int _RemainingThrottledCommands;
	// private static readonly ReaderWriterLockSlim _

	internal ChatClient(string channelName, string chatBotName, string oauthToken, ILogger logger)
	{

		ChannelName = channelName;
		ChatBotName = chatBotName;
		_OAuthToken = oauthToken;
		Logger = logger;

		SetRegularExpressionForChatMessages();

		_Shutdown = new CancellationTokenSource();

	}

	private void SetRegularExpressionForChatMessages()
	{
		reChatMessage = new Regex($@"PRIVMSG #{ChannelName} :(.*)$");
		reWhisperMessage = new Regex($@"WHISPER {ChatBotName} :(.*)$");
	}

	~ChatClient()
	{

		try
		{
			Logger?.LogError("GC the ChatClient");
		}
		catch { }
		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		Dispose(false);
	}

	public void Init()
	{

		_Shutdown.TryReset();

		Connect();

		_ReceiveMessagesThread = new Thread(ReceiveMessagesOnThread);
		_ReceiveMessagesThread.Start();

	}

	public ILogger Logger { get; }

	public string ChannelName { get; private set; }
	public string ChatBotName { get; }
	public bool IsRunning => _ReceiveMessagesThread.IsAlive;

	public bool IsConnected { get; private set; }

	private readonly string _OAuthToken;
	private readonly CancellationTokenSource _Shutdown;

	private void Connect()
	{

		_TcpClient = new TcpClient("irc.chat.twitch.tv", 6667);

		inputStream = new StreamReader(_TcpClient.GetStream());
		outputStream = new StreamWriter(_TcpClient.GetStream());

		Logger.LogTrace("Beginning IRC authentication to Twitch");
		outputStream.WriteLine("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
		outputStream.WriteLine($"PASS oauth:{_OAuthToken}");
		outputStream.WriteLine($"NICK {ChatBotName}");
		outputStream.WriteLine($"USER {ChatBotName} 8 * :{ChatBotName}");
		outputStream.Flush();

		outputStream.WriteLine($"JOIN #{ChannelName}");
		outputStream.Flush();

		//Connected?.Invoke(this, new ChatConnectedEventArgs());

	}

	private void SendMessage(string message, bool flush = true)
	{

		var throttled = CheckThrottleStatus();

		Thread.Sleep(throttled.GetValueOrDefault(TimeSpan.FromSeconds(0)));

		outputStream.WriteLine(message);
		if (flush)
		{
			outputStream.Flush();
		}

	}

	private TimeSpan? CheckThrottleStatus()
	{

		var throttleDuration = TimeSpan.FromSeconds(30);
		var maximumCommands = 100;

		if (_NextReset == null)
		{
			_NextReset = DateTime.UtcNow.Add(throttleDuration);
		}
		else if (_NextReset < DateTime.UtcNow)
		{
			_NextReset = DateTime.UtcNow.Add(throttleDuration);
		}

		// TODO: Finish checking and enforcing the chat throttling

		return null;


	}

	/// <summary>
	/// Public async interface to post messages to channel
	/// </summary>
	/// <param name="message"></param>
	public void PostMessage(string message)
	{

		var fullMessage = $":{ChatBotName}!{ChatBotName}@{ChatBotName}.tmi.twitch.tv PRIVMSG #{ChannelName} :{message}";

		SendMessage(fullMessage);

	}

	public void WhisperMessage(string message, string userName)
	{

		var fullMessage = $":{ChatBotName}!{ChatBotName}@{ChatBotName}.tmi.twitch.tv PRIVMSG #jtv :/w {userName} {message}";
		SendMessage(fullMessage);

	}

	private void ReceiveMessagesOnThread()
	{

		var lastMessageReceivedTimestamp = DateTime.Now;
		var errorPeriod = TimeSpan.FromSeconds(60);

		while (!_Shutdown.IsCancellationRequested)
		{

			Thread.Sleep(50);

			if (DateTime.Now.Subtract(lastMessageReceivedTimestamp) > errorPeriod)
			{
				Logger.LogError($"Haven't received a message in {errorPeriod.TotalSeconds} seconds");
				lastMessageReceivedTimestamp = DateTime.Now;
			}

			if (_TcpClient.Connected && _TcpClient.Available > 0)
			{

				var msg = ReadMessage();
				if (string.IsNullOrEmpty(msg))
				{
					continue;
				}

				lastMessageReceivedTimestamp = DateTime.Now;
				Logger.LogTrace($"> {msg}");

				// Handle the Twitch keep-alive
				if (msg.StartsWith("PING"))
				{
					Logger.LogWarning("Received PING from Twitch... sending PONG");
					SendMessage($"PONG :{msg.Split(':')[1]}");
					continue;
				}

				ProcessMessage(msg);

			}
			else if (!_TcpClient.Connected)
			{
				// Reconnect
				Logger.LogWarning("Disconnected from Twitch.. Reconnecting in 2 seconds");
				Thread.Sleep(2000);
				Init();
				return;
			}

		}

		try
		{
			Logger.LogWarning("Exiting ReceiveMessages Loop");
		}
		catch
		{
			// Error while shutting down
		}

	}

	private void ProcessMessage(string msg)
	{

		Logger.LogError("Processing message: " + msg);

		var userName = "";
		var message = "";

		userName = ChatClient.reUserName.Match(msg).Groups[1].Value;
		//if (userName.Equals(ChatBotName, StringComparison.InvariantCultureIgnoreCase)) return; // Exit and do not process if the bot posted this message


		if (msg.Contains($"{ChatBotName} :Welcome, GLHF!", StringComparison.InvariantCultureIgnoreCase))
		{
			IsConnected = true;
		}

		// Review messages sent to the channel
		if (reChatMessage.IsMatch(msg))
		{

			var displayName = ChatClient.reDisplayName.Match(msg).Groups[1].Value;
			var timestamp = long.Parse(ChatClient.reTimestamp.Match(msg).Groups[1].Value);
			var messageId = ChatClient.reMessageId.Match(msg).Groups[1].Value;

			var badges = ChatClient.reBadges.Match(msg).Groups[1].Value.Split(',');

			// Handle Emotes
			List<Emote> emotes = IdentifyEmotes(msg);

			message = ChatClient.reChatMessage.Match(msg).Groups[1].Value;
			Logger.LogTrace($"Message received from '{userName}': {message}");
			NewMessage?.Invoke(this, new NewMessageEventArgs
			{
				MessageId = messageId,
				UserName = userName,
				DisplayName = displayName,
				Message = message,
				Badges = badges,
				Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp),
				Emotes = emotes.ToArray()
			});

		}
		//else if (reWhisperMessage.IsMatch(msg))
		//{

		//	message = ChatClient.reWhisperMessage.Match(msg).Groups[1].Value;
		//	Logger.LogTrace($"Whisper received from '{userName}': {message}");

		//	NewMessage?.Invoke(this, new NewMessageEventArgs
		//	{
		//		UserName = userName,
		//		Message = message,
		//		Badges = (badges ?? new string[] { }),
		//		IsWhisper = true
		//	});

		//}

	}

	internal static List<Emote> IdentifyEmotes(string msg)
	{
		var emotesRaw = ChatClient.reEmotes.Match(msg).Groups[1].Value.Replace(';', ' ').Split('/', StringSplitOptions.TrimEntries);
		var emotes = new List<Emote>(emotesRaw.Length);
		if ((emotesRaw?.Any() ?? false) && !string.IsNullOrEmpty(emotesRaw.First()))
		{
			foreach (var emote in emotesRaw)
			{
				var parts = emote.Split(":");
				var entries = parts[1].Split(",");
				foreach (var entry in entries)
				{
					var positions = entry.Split("-");
					emotes.Add(new Emote(int.Parse(positions[0]), int.Parse(positions[1]) - int.Parse(positions[0]) + 1, $"https://static-cdn.jtvnw.net/emoticons/v2/{parts[0]}/static/light/2.0"));
				}
			}
		}

		return emotes;
	}

	private string ReadMessage()
	{

		string message = null;

		try
		{
			message = inputStream.ReadLine();
		}
		catch (Exception ex)
		{
			Logger.LogError("Error reading messages: " + ex);
		}

		return message ?? "";

	}

	#region IDisposable Support
	private bool disposedValue = false; // To detect redundant calls
	private Thread _ReceiveMessagesThread;

	protected virtual void Dispose(bool disposing)
	{

		try
		{
			Logger?.LogWarning("Disposing of ChatClient");
		}
		catch { }

		if (!disposedValue)
		{
			if (disposing)
			{
				_Shutdown.Cancel();
			}

			_TcpClient?.Dispose();
			disposedValue = true;
		}
	}

	// This code added to correctly implement the disposable pattern.
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public void Stop()
	{
		_Shutdown?.Cancel();
	}

	public void ListenToNewChannel(string channelName)
	{

		// Issue IRC PART and JOIN commands to switch channels
		SendMessage($"PART #{ChannelName}");
		SendMessage($"JOIN #{channelName}");

		ChannelName = channelName;

		SetRegularExpressionForChatMessages();

	}

	#endregion
}

public static class BufferHelpers
{

	public static ArraySegment<byte> ToBuffer(this string text)
	{

		return Encoding.UTF8.GetBytes(text);

	}

}
