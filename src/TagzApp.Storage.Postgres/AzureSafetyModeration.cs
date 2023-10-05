using Azure;
using Azure.AI.ContentSafety;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TagzApp.Web.Services;

namespace TagzApp.Storage.Postgres;

internal class AzureSafetyModeration : INotifyNewMessages
{
	private INotifyNewMessages _NotifyNewMessages;
	private readonly IConfiguration _Configuration;
	private readonly ILogger<AzureSafetyModeration> _AzureSafetyLogger;
	private readonly bool _Enabled;
	private readonly string? _ContentSafetyKey;
	private readonly string? _ContentSafetyEndpoint;

	public AzureSafetyModeration(
		INotifyNewMessages notifyNewMessages,
		IConfiguration configuration,
		ILogger<AzureSafetyModeration> azureSafetyLogger)
	{

		_NotifyNewMessages = notifyNewMessages;
		_Configuration = configuration;
		_AzureSafetyLogger = azureSafetyLogger;

		_Enabled = _Configuration["AzureContentSafetyKey"] != null;

		_ContentSafetyKey = _Configuration["AzureContentSafetyKey"];
		_ContentSafetyEndpoint = _Configuration["AzureContentSafetyEndpoint"];

	}

	public void NotifyApprovedContent(string hashtag, Content content, ModerationAction action)
	{

		_NotifyNewMessages.NotifyApprovedContent(hashtag, content, action);
		return;

	}

	public void NotifyRejectedContent(string hashtag, Content content, ModerationAction action)
	{

		_NotifyNewMessages.NotifyRejectedContent(hashtag, content, action);
		return;

	}

	public void NotifyNewContent(string hashtag, Content content)
	{

		if (!_Enabled)
		{
			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			return;
		}

		// use Azure Content Safety API to check the content
		// if it's unsafe, add a moderation action that indicates Azure rejected it
		// and log the rejection

		var client = new ContentSafetyClient(new Uri(_ContentSafetyEndpoint), new AzureKeyCredential(_ContentSafetyKey));

		var request = new AnalyzeTextOptions(content.Text);

		Response<AnalyzeTextResult> response;
		try
		{
			response = client.AnalyzeText(request);
		}
		catch (RequestFailedException ex)
		{
			_AzureSafetyLogger.LogError(ex, "Analyze text failed.\nStatus code: {0}, Error code: {1}, Error message: {2}", ex.Status, ex.ErrorCode, ex.Message);
			throw;
		}

		response.Value.HateResult.Category


	}

}
