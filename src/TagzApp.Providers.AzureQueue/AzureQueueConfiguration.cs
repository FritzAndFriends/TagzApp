namespace TagzApp.Providers.AzureQueue;

public class AzureQueueConfiguration : IProviderConfiguration
{

	public string QueueConnectionString { get; } = string.Empty;

	public string Name => "AzureQueue";

	public string Description => "Reads messages from a custom Azure Queue";

	public bool Enabled { get; set; } = false;

}

