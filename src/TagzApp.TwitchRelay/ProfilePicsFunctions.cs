using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TagzApp.TwitchRelay;

public class ProfilePicsFunctions
{
	private readonly ILogger _logger;
	private readonly TwitchProfileRepository _Repository;

	public ProfilePicsFunctions(TwitchProfileRepository repository, ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<ChatUrlManagement>();
		_Repository = repository;
	}

	[Function("ProfilePics")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ProfilePics/{userNames}")] HttpRequestData req,
		string userNames
	)
	{

		var response = req.CreateResponse(HttpStatusCode.OK);

		var names = userNames.ToLowerInvariant().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		var users = await _Repository.GetProfilePics(names);
		await response.WriteAsJsonAsync(users);

		return response;
	}
}

