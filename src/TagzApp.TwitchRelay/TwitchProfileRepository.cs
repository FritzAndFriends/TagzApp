using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using TagzApp.TwitchRelay.Data;

namespace TagzApp.TwitchRelay;

internal class TwitchProfileRepository
{

	private static readonly ConcurrentDictionary<string, (string, DateTime)> _ProfilePics = new();
	private readonly string _ClientId;
	private readonly string _ClientSecret;
	private readonly HttpClient _HttpClient;
	private string _AccessToken = string.Empty;

	public TwitchProfileRepository(string clientId, string clientSecret, HttpClient client)
	{
		_ClientId = clientId;
		_ClientSecret = clientSecret;
		_HttpClient = client;
	}

	public async Task<string> GetProfilePic(string userName)
	{

		if (_ProfilePics.ContainsKey(userName))
		{
			var (profilePic, expiry) = _ProfilePics[userName];

			if (expiry > DateTime.UtcNow)
			{
				return profilePic;
			}
		}

		var profilePicUrl = await GetProfilePicFromTwitch(userName);

		_ProfilePics.AddOrUpdate(userName, (profilePicUrl, DateTime.UtcNow.AddHours(1)), (key, oldValue) => (profilePicUrl, DateTime.UtcNow.AddHours(1)));

		return profilePicUrl;

	}

	public async Task SeedProfilePics(IEnumerable<string> userNames)
	{

		await GetAccessToken();

		var now = DateTime.UtcNow;
		for (var i = 0; i < userNames.Count(); i += 100)
		{

			// Request twitch profile pics in batches of 100
			var batch = userNames.Skip(i).Take(100);
			var request = new HttpRequestMessage(
							HttpMethod.Get,
							$"https://api.twitch.tv/helix/users?login={string.Join("&login=", batch)}");

			var response = await _HttpClient.SendAsync(request);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var users = JsonSerializer.Deserialize<TwitchUsers>(content);

				foreach (var user in users.data)
				{
					_ProfilePics.AddOrUpdate(user.login, (user.profile_image_url, now.AddHours(1)), (key, oldValue) => (user.profile_image_url, now.AddHours(1)));
				}
			}
			else
			{
				throw new Exception("Failed to get users");
			}


		}

	}

	private async Task GetAccessToken()
	{

		if (!string.IsNullOrEmpty(_AccessToken)) return;

		var request = new HttpRequestMessage(
			HttpMethod.Post,
			"https://id.twitch.tv/oauth2/token");
		request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
			{ "client_id", _ClientId },
			{ "client_secret", _ClientSecret },
			{ "grant_type", "client_credentials" },
		});

		var response = await _HttpClient.SendAsync(request);

		if (response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync();
			var token = JsonSerializer.Deserialize<AccessToken>(content);
			_AccessToken = token.access_token;
			_HttpClient.DefaultRequestHeaders.Clear();
			_HttpClient.DefaultRequestHeaders.Add("Client-Id", _ClientId);
			_HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _AccessToken);
		}
		else
		{
			throw new Exception("Failed to get access token");
		}

	}

	private async Task<string> GetProfilePicFromTwitch(string userName)
	{

		await SeedProfilePics(new[] { userName });

		return _ProfilePics[userName].Item1;

	}

}

