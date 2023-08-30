using TagzApp.Common.Models;
using TagzApp.Providers.Blazot.Models;

namespace TagzApp.Providers.Blazot.Interfaces;

internal interface ITransmissionsService
{
  bool HasMadeTooManyRequests { get; }
  Task StartInitialWindowTimerAsync();
  Task<List<Transmission>?> GetHashtagTransmissionsAsync(Hashtag tag, DateTimeOffset dateTimeOffset);
}
