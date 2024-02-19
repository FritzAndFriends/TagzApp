using System.Net;

namespace TagzApp.Providers.Blazot.Interfaces;

public interface IAuthService
{
	string? AccessToken { get; }
	Task<(bool? isSuccessStatusCode, HttpStatusCode? httpStatusCode)> GetAccessTokenAsync();
}
