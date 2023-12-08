// See https://aka.ms/new-console-template for more information
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
Console.WriteLine("Hello, World!");

const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=dotnetconf;AccountKey=XcBjXGdyMcoqyB29cC+QLfHhuEhGCJurpCYwRI1I78958t5rRBrDS8fgftMPe4xKwnVpgz8Yu2Of9AluOUtqfA==;EndpointSuffix=core.windows.net";

var client = new QueueClient(ConnectionString, "tagzapp-content");

var newMessage = new Content
{
	Author = new Creator
	{
		DisplayName = "Test User",
		ProfileImageUri = new Uri("https://bing.com"),
		ProfileUri = new Uri("https://bing.com"),
	},
	Provider = "WEBSITE",
	ProviderId = Guid.NewGuid().ToString(),
	SourceUri = new Uri("https://bing.com"),
	Text = "This is a test message",
	Timestamp = DateTimeOffset.UtcNow,
};

var msgJson = JsonSerializer.Serialize(newMessage);

await client.SendMessageAsync(msgJson);
