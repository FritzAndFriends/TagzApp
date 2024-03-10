using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TagzApp.TwitchRelay;

public class Function1
{
	private readonly IConfiguration _Configuration;
	private readonly ILogger _logger;

	public Function1(IConfiguration configuration, ILoggerFactory loggerFactory)
	{
		_Configuration = configuration;
		_logger = loggerFactory.CreateLogger<Function1>();
	}

	[Function("Function1")]
	public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
	{
		_logger.LogInformation("C# HTTP trigger function processed a request.");

		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

		response.WriteString("Welcome to Azure Functions!");

		return response;
	}

	[Function("GetLoginUrl")]
	public HttpResponseData GetLoginUrl(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetLoginUrl")] HttpRequestData req
	)
	{
		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

		response.WriteString($"https://id.twitch.tv/oauth2/authorize?" +
			$"client_id={_Configuration["TwitchClientId"]}&" +
			$"redirect_uri={_Configuration["TwitchRedirectUri"]}&" +
			"response_type=code&scope=channel:bot+user:bot+chat:read+user:read:chat");

		return response;

	}

}
