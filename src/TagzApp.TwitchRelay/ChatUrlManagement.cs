using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TagzApp.TwitchRelay.Data;

namespace TagzApp.TwitchRelay;

public class ChatUrlManagement
{
	private readonly IConfiguration _Configuration;
	private readonly HttpClient _Client;
	private readonly ILogger _logger;
	private static readonly Dictionary<Guid, (string, DateTime)> _Redirects = new();

	public ChatUrlManagement(IConfiguration configuration, IHttpClientFactory clientFactory, ILoggerFactory loggerFactory)
	{
		_Configuration = configuration;
		_Client = clientFactory.CreateClient();
		_logger = loggerFactory.CreateLogger<ChatUrlManagement>();

		foreach (var key in _Redirects.Keys)
		{

			// Remove old redirects
			if (_Redirects.ContainsKey(key) && _Redirects[key].Item2 < DateTime.Now)
			{
				try
				{
					_Redirects.Remove(key);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error removing old redirect");
				}
			}

		}

	}

	[Function("GetLoginUrl")]
	public HttpResponseData GetLoginUrl(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetLoginUrl")] HttpRequestData req
	)
	{

		var redirect = req.Query["returnto"];
		if (string.IsNullOrEmpty(redirect)) return req.CreateResponse(HttpStatusCode.BadRequest);
		var thisId = Guid.NewGuid();
		_Redirects.TryAdd(thisId, (redirect!, DateTime.Now.AddMinutes(5)));

		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

		_logger.LogInformation($"Generating login url to returnto: {redirect}");

		string redirectUrl = $"https://id.twitch.tv/oauth2/authorize?" +
					$"client_id={_Configuration["TwitchClientId"]}&" +
					$"redirect_uri={_Configuration["TwitchRedirectUri"]}&" +
					$"state={thisId}&" +
					"response_type=token&scope=channel:bot+user:bot+chat:read+user:read:chat";

		_logger.LogInformation($"Completed URL: {redirectUrl}");
		Console.WriteLine($"Completed URL: {redirectUrl}");

		response.WriteString(redirectUrl);

		return response;

	}

	[Function("GetRedirect")]
	public HttpResponseData GetRedirect(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetRedirect/{id}")] HttpRequestData req,
					string id
						)
	{
		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

		if (_Redirects.ContainsKey(Guid.Parse(id)))
		{
			response.WriteString(_Redirects[Guid.Parse(id)].Item1);
			_Redirects.Remove(_Redirects.First(x => x.Key == Guid.Parse(id)).Key);
		}
		else
		{
			response.WriteString("Invalid ID");
		}

		return response;
	}

	[Function("GetTwitchUser")]
	public async Task<HttpResponseData> GetTwitchUser(
				[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetTwitchUser/{access_token}")] HttpRequestData req,
									string access_token
															)
	{
		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

		_Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {access_token}");
		_Client.DefaultRequestHeaders.Add("Client-Id", _Configuration["TwitchClientId"]);
		var result = await _Client.GetAsync("https://api.twitch.tv/helix/users");
		var content = await result.Content.ReadAsStringAsync();

		var twitchUser = JsonSerializer.Deserialize<TwitchUsers>(content).data.First();

		response.WriteString(twitchUser.display_name);

		return response;
	}


	[Function("twitchcallback")]
	public HttpResponseData TwitchCallback(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "twitchcallback")] HttpRequestData req
				)
	{
		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "text/html; charset=utf-8");

		if (!string.IsNullOrEmpty(req.Query["error"]))
		{
			response.WriteString($"<html><body><span class='alert-danger'>Error while connecting to Twitch: {req.Query["error"]}");
			return response;
		}

		// Capture the access code from the fragment after the # in the URL and in the access_code key value pair
		response.WriteString(
			"""
				<html><head>
						<script>
							var hash = window.location.hash;
							var access_code = hash.split('access_token=')[1].split('&')[0];
							var state = hash.split('state=')[1].split('&')[0];

							// Get the twitch user name from the server
							var xhr = new XMLHttpRequest();
							var twitchUser = "";
							xhr.open('GET', '/api/GetTwitchUser/' + access_code, true);
							xhr.send();
							xhr.onreadystatechange = function() {
								if (xhr.readyState == 4 && xhr.status == 200) {
									twitchUser = xhr.responseText;

								// Get the redirect URL from the server
								urlxhr = new XMLHttpRequest();
								urlxhr.open('GET', '/api/GetRedirect/' + state, true);
								urlxhr.send();
								urlxhr.onreadystatechange = function() {
									if (urlxhr.readyState == 4 && urlxhr.status == 200) {
										var redirect = urlxhr.responseText;
										window.location.href = `${redirect}?access_token=${access_code}&user=${twitchUser}`;
									}
								};
								}
							};
						</script>
					</head>
				</html>
			"""
		);

		return response;
	}

}
