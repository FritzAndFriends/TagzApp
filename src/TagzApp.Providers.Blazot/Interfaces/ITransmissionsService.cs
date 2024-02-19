using TagzApp.Providers.Blazot.Models;

namespace TagzApp.Providers.Blazot.Interfaces;

public interface ITransmissionsService
{
	bool HasMadeTooManyRequests { get; }
	Task StartInitialWindowTimerAsync();
	Task<List<Transmission>?> GetHashtagTransmissionsAsync(Hashtag tag, DateTimeOffset dateTimeOffset);
}
