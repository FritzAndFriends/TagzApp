using System.Diagnostics.Metrics;

namespace TagzApp.Common.Telemetry;

public class ProviderInstrumentation
{
	public Counter<int> MessagesReceivedCounter { get; set; }
	public Counter<int> ConnectionStatusChangesCounter { get; set; }
	public ObservableGauge<int> ConnectionStatusGauge { get; set; }

	private readonly Dictionary<string, int> _ProviderConnectionStatus = new();
	private readonly object _StatusLock = new();

	public ProviderInstrumentation(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("tagzapp-provider-metrics");

		MessagesReceivedCounter = meter.CreateCounter<int>("messages-received", "message", "Counter for messages received");
		ConnectionStatusChangesCounter = meter.CreateCounter<int>("connection-status-changes", "change", "Counter for connection status changes");
		ConnectionStatusGauge = meter.CreateObservableGauge("connection-status", () => GetConnectionStatusMeasurements(), "status", "Current connection status of providers (0=Disabled, 1=Unhealthy, 2=Degraded, 3=Healthy)");
	}

	public void AddMessage(string provider, string author) =>
		MessagesReceivedCounter.Add(1,
			new KeyValuePair<string, object?>(nameof(provider), provider),
			new KeyValuePair<string, object?>(nameof(author), author));

	public void RecordConnectionStatusChange(string provider, SocialMediaStatus status)
	{
		lock (_StatusLock)
		{
			_ProviderConnectionStatus[provider] = (int)status;
		}

		ConnectionStatusChangesCounter.Add(1,
			new KeyValuePair<string, object?>(nameof(provider), provider),
			new KeyValuePair<string, object?>(nameof(status), status.ToString()));
	}

	private IEnumerable<Measurement<int>> GetConnectionStatusMeasurements()
	{
		lock (_StatusLock)
		{
			foreach (var kvp in _ProviderConnectionStatus)
			{
				yield return new Measurement<int>(kvp.Value, new KeyValuePair<string, object?>("provider", kvp.Key));
			}
		}
	}
}
