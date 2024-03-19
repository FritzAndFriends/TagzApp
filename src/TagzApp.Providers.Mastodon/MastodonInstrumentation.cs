using System.Diagnostics.Metrics;

namespace TagzApp.Providers.Mastodon;

public class MastodonInstrumentation
{
	private Counter<int> MessagesReceivedCounter { get; }
	private Counter<int> MessagesByAuthorCounter { get; }

	public MastodonInstrumentation(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("mastodon-metrics");

		MessagesReceivedCounter = meter.CreateCounter<int>("mastodon-messages-received", "message", "Counter for Mastodon Messages Received");
		MessagesByAuthorCounter = meter.CreateCounter<int>("mastodon-messages-by-author", "message", "Counter for Mastodon Messages Received by Author");
	}

	public void AddMessages(int count) => MessagesReceivedCounter.Add(count);
	public void AddMessages(string author) => MessagesByAuthorCounter.Add(1, new KeyValuePair<string, object?>("author", author));
}
