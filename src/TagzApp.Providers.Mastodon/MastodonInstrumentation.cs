using System.Diagnostics.Metrics;

namespace TagzApp.Providers.Mastodon;

public class MastodonInstrumentation
{
	private Counter<int> MessagesReceivedCounter { get; }

	public MastodonInstrumentation(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("mastodon-metrics");

		MessagesReceivedCounter = meter.CreateCounter<int>("mastodon-messages-received", "message", "Counter for Mastodon Messages Received");
	}

	public void AddMessages(int count) => MessagesReceivedCounter.Add(count);
}
