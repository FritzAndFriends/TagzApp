using System.Diagnostics.Metrics;

namespace TagzApp.Providers.TwitchChat;

public class TwitchChatInstrumentation
{
	private Counter<int> MessagesReceivedCounter { get; }
	private Counter<int> MessagesByAuthorCounter { get; }

	public TwitchChatInstrumentation(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("twitchchat-metrics");

		MessagesReceivedCounter = meter.CreateCounter<int>("twitchchat-messages-received", "message", "Counter for TwitchChat Messages Received");
		MessagesByAuthorCounter = meter.CreateCounter<int>("twitchchat-messages-by-author", "message", "Counter for TwitchChat Messages Received by Author");
	}

	public void AddMessages(int count) => MessagesReceivedCounter.Add(count);
	public void AddMessages(string author) => MessagesByAuthorCounter.Add(1, new KeyValuePair<string, object?>("author", author));
}
