namespace TagzApp.Providers.AzureQueue;

public class AzureQueueConfiguration : IProviderConfiguration
{

	public string QueueConnectionString { get; private set; } = string.Empty;

	public string Name => "AzureQueue";

	public string Description => "Reads messages from a custom Azure Queue";

	public bool Enabled { get; set; } = false;

	public string[] Keys => ["QueueConnectionString"];

	public string GetConfigurationByKey(string key)
	{

		return key switch
		{
			"Name" => Name,
			"Description" => Description,
			"Enabled" => Enabled.ToString(),
			"QueueConnectionString" => QueueConnectionString,
			_ => throw new NotImplementedException($"The key '{key}' is not available")
		};

	}

	public void SetConfigurationByKey(string key, string value)
	{

		if (key == "QueueConnectionString")
		{
			QueueConnectionString = value;
			return;
		}

		throw new NotImplementedException();


	}
}

