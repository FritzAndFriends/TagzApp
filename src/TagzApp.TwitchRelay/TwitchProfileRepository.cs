using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using TagzApp.TwitchRelay.Data;

namespace TagzApp.TwitchRelay;

public class TwitchProfileRepository
{

	private readonly string _ClientId;
	private readonly string _ClientSecret;
	private readonly HttpClient _HttpClient;
	private readonly IDatabase _Cache;
	private static string _AccessToken = string.Empty;

	public TwitchProfileRepository(IConfiguration configuration, IHttpClientFactory clientFactory, IConnectionMultiplexer connectionMultiplexer)
	{
		_ClientId = configuration["TwitchClientId"];
		_ClientSecret = configuration["TwitchSecret"];
		_HttpClient = clientFactory.CreateClient();

		_Cache = connectionMultiplexer.GetDatabase();

	}

	private string FormatCacheKey(string userName) => $"TwitchProfile:{userName.ToLowerInvariant()}";

	private string GetUserNameFromCacheKey(string cacheKey) => cacheKey.Split(':')[1];

	public async Task<string> GetProfilePic(string userName)
	{

		var profile = await _Cache.StringGetAsync(FormatCacheKey(userName));
		if (profile.IsNull)
		{
			await SeedProfilePics(new[] { userName });
			profile = await _Cache.StringGetAsync(FormatCacheKey(userName));
		}

		if (string.IsNullOrEmpty(profile)) return string.Empty;

		var user = JsonSerializer.Deserialize<TwitchUser>(profile.ToString());
		return user.profile_image_url;

	}


	public async Task<Dictionary<string, string>> GetProfilePics(IEnumerable<string> userNames)
	{

		var outList = new ConcurrentDictionary<string, string>();

		var names = userNames.Select(u => FormatCacheKey(u)).Distinct().Select(u => new RedisKey(u)).ToArray();

		var profiles = await _Cache.StringGetAsync(names);
		await Parallel.ForEachAsync(profiles, async (profile, token) =>
		{

			if (!profile.IsNull)
			{
				var user = JsonSerializer.Deserialize<TwitchUser>(profile.ToString());
				outList.AddOrUpdate(user.login, user.profile_image_url, (k, v) => user.profile_image_url);
			}

		});

		var usersToFetch = userNames.Except(outList.Keys.Select(k => GetUserNameFromCacheKey(k))).ToList();
		if (usersToFetch.Any())
		{

			await SeedProfilePics(usersToFetch);

			names = usersToFetch.Select(u => FormatCacheKey(u)).Distinct().Select(u => new RedisKey(u)).ToArray();
			profiles = await _Cache.StringGetAsync(names);
			await Parallel.ForEachAsync(profiles, async (profile, token) =>
			{

				if (!profile.IsNull)
				{
					var user = JsonSerializer.Deserialize<TwitchUser>(profile.ToString());
					outList.AddOrUpdate(user.login, user.profile_image_url, (k, v) => user.profile_image_url);
				}

			});

		}

		return outList.ToDictionary();

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
					await _Cache.StringSetAsync(FormatCacheKey(user.login), JsonSerializer.Serialize(user), TimeSpan.FromMinutes(30));
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

		if (!string.IsNullOrEmpty(_AccessToken))
		{
			_HttpClient.DefaultRequestHeaders.Clear();
			_HttpClient.DefaultRequestHeaders.Add("Client-Id", _ClientId);
			_HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _AccessToken);
			return;
		}

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

}

