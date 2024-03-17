namespace TagzApp.TwitchRelay.Data;

internal class AccessToken
{

	public required string access_token { get; set; }

	public required long expires_in { get; set; }

	public required string token_type { get; set; }

}

