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
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(3);

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

			var rawContent = JsonSerializer.Deserialize<QuestionInputModel>(msg.Body.ToStream());
			var content = (Content)rawContent;
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



public class QuestionInputModel
{

	public string Author { get; set; }

	public string Text { get; set; }

	public static explicit operator Content(QuestionInputModel model)
	{

		// Convert to a content object
		return new Content
		{

			Author = new Creator
			{
				ProfileImageUri = new Uri("https://bing.com"),
				ProfileUri = new Uri("https://bing.com"),
				DisplayName = model.Author,
				ProviderId = "WEBSITE",
				UserName = model.Author.ToLowerInvariant()
			},

			Provider = "WEBSITE",
			ProviderId = Guid.NewGuid().ToString(),
			SourceUri = new Uri("https://dotnetconf.net"),
			Text = model.Text,
			Timestamp = DateTimeOffset.UtcNow,
			Type = ContentType.Message

		};

	}

}
