using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TagzApp.Providers.Kick;

internal class KickProfileRepository
{
	private static readonly ConcurrentDictionary<string, (string, DateTime)> _ProfilePics = new();
	private readonly HttpClient _HttpClient;

	public KickProfileRepository(IConfiguration configuration, HttpClient client)
	{
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

		var profilePicUrl = await GetProfilePicFromKick(userName);

		_ProfilePics.AddOrUpdate(userName, (profilePicUrl, DateTime.UtcNow.AddHours(1)), (key, oldValue) => (profilePicUrl, DateTime.UtcNow.AddHours(1)));

		return profilePicUrl;
	}

	private async Task<string> GetProfilePicFromKick(string userName)
	{
		try
		{
			// Kick.com API endpoint for user information
			var response = await _HttpClient.GetAsync($"https://kick.com/api/v1/users/{userName}");

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				using var doc = JsonDocument.Parse(content);

				if (doc.RootElement.TryGetProperty("profile_pic", out var profilePicElement))
				{
					var profilePicUrl = profilePicElement.GetString();
					if (!string.IsNullOrEmpty(profilePicUrl))
					{
						return profilePicUrl;
					}
				}
			}
		}
		catch (Exception)
		{
			// If we can't get the profile pic, return a default
		}

		// Return default avatar if we can't get the user's profile pic
		return "https://kick.com/images/default-avatar.png";
	}
}
