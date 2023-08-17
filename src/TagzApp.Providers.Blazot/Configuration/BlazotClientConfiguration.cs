using TagzApp.Communication.Configuration;

namespace TagzApp.Providers.Blazot.Configuration;

public class BlazotClientConfiguration : HttpClientOptions
{
  /// <summary>
  /// Declare the section name used.
  /// </summary>
  public const string AppSettingsSection = "providers:blazot";

  /// <summary>
  /// Blazot issued API Key.
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;
}
