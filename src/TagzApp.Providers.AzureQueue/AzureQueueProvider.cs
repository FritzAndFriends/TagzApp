using Azure.Storage.Queues;
using System.Text.Json;

namespace TagzApp.Providers.AzureQueue;

public class AzureQueueProvider : ISocialMediaProvider
{
	private const string QueueName = "tagzapp-content";
	private readonly AzureQueueConfiguration _Configuration;
	private QueueClient _Client;
	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	public string Id => "WEBSITE";
	public string DisplayName => "Website";
	public string DllName { get { return "AzureQueue"; } }
	public string Description => "Q+A submitted through a website form";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(5);

	public AzureQueueProvider(AzureQueueConfiguration configuration)
	{
		_Configuration = configuration;
	}


	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		var messageResponse = await _Client.ReceiveMessagesAsync(maxMessages: 10);
		if (!messageResponse.Value.Any()) return Enumerable.Empty<Content>();

		var outList = new List<Content>();

		foreach (var msg in messageResponse.Value)
		{

			var content = JsonSerializer.Deserialize<Content>(msg.Body.ToStream());
			if (content is not null)
			{
				content.HashtagSought = tag.Text.ToLowerInvariant();
				outList.Add(content);
				await _Client.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
			}

		}

		return outList;


	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth()
	{
		return Task.FromResult((_Status, _StatusMessage));
	}

	public async Task StartAsync()
	{

		_Client = new QueueClient(_Configuration.QueueConnectionString, QueueName);

		try
		{
			await _Client.CreateIfNotExistsAsync();
		}
		catch (Exception ex)
		{
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Unable to start a connection to the Azure Queue: {ex.Message}";
			return;
		}

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = "Connected";

	}
}
