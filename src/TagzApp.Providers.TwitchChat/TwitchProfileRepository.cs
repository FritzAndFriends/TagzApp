using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TagzApp.Providers.TwitchChat;
internal class TwitchProfileRepository
{

	private static readonly ConcurrentDictionary<string, (string, DateTime)> _ProfilePics = new();
	private readonly string _ClientId;
	private readonly string _ClientSecret;
	private readonly HttpClient _HttpClient;
	private string _AccessToken = string.Empty;
	private string _RelayUri = string.Empty;

	public TwitchProfileRepository(IConfiguration configuration, HttpClient client)
	{
		_HttpClient = client;
		_RelayUri = configuration["TwitchRelayUri"];
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

		var now = DateTime.UtcNow;
		for (var i = 0; i < userNames.Count(); i += 100)
		{

			// Request twitch profile pics in batches of 100
			var batch = userNames.Skip(i).Take(100);
			var request = new HttpRequestMessage(
							HttpMethod.Get,
							$"{_RelayUri}/api/ProfilePics/{string.Join("&login=", batch)}");

			var response = await _HttpClient.SendAsync(request);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var users = JsonSerializer.Deserialize<Dictionary<string, string>>(content);

				foreach (var user in users)
				{
					_ProfilePics.AddOrUpdate(user.Key, (user.Value, now.AddHours(1)), (key, oldValue) => (user.Value, now.AddHours(1)));
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

internal class AccessToken
{

	public required string access_token { get; set; }

	public required long expires_in { get; set; }

	public required string token_type { get; set; }

}


public class TwitchUsers
{
	public TwitchUser[] data { get; set; }
}

public class TwitchUser
{
	public string id { get; set; }
	public string login { get; set; }
	public string display_name { get; set; }
	public string type { get; set; }
	public string broadcaster_type { get; set; }
	public string description { get; set; }
	public string profile_image_url { get; set; }
	public string offline_image_url { get; set; }
	public int view_count { get; set; }
	public string email { get; set; }
	public DateTime created_at { get; set; }
}
