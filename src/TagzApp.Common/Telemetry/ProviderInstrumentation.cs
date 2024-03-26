using System.Diagnostics.Metrics;

namespace TagzApp.Common.Telemetry;

public class ProviderInstrumentation
{
	public Counter<int> MessagesReceivedCounter { get; set; }

	public ProviderInstrumentation(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("tagzapp-provider-metrics");

		MessagesReceivedCounter = meter.CreateCounter<int>("messages-received", "message", "Counter for messages received");
	}

	public void AddMessage(string provider, string author) =>
		MessagesReceivedCounter.Add(1,
			new KeyValuePair<string, object?>(nameof(provider), provider),
			new KeyValuePair<string, object?>(nameof(author), author));
}
