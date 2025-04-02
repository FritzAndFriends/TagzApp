using Azure.Storage.Queues;
using System.Text.Json;

namespace TagzApp.Lib.AzureQueue;

/// <summary>
/// Client for the Azure Queue service provider for use with TagzApp
/// </summary>
/// <param name="ConnectionString">Connectionstring to the Azure Storage service</param>
/// <param name="QueueName">Name of the Azure queue to work with</param>
public class MessageClient(string ConnectionString, string QueueName)
{

	private QueueClient GetQueueClient()
	{
		var queueClient = new QueueClient(ConnectionString, QueueName);
		queueClient.CreateIfNotExists();
		return queueClient;
	}

	public async Task SubmitMessage(string Message, string Author)
	{
		var queueClient = GetQueueClient();

		var theMessage = new
		{
			Text = Message,
			Author
		};

		var content = JsonSerializer.Serialize(theMessage);
		await queueClient.SendMessageAsync(content);
	}

}
