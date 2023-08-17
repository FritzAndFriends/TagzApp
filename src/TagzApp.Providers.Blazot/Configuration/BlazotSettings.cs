using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagzApp.Providers.Blazot.Configuration;
internal class BlazotSettings
{
  /// <summary>
  /// Declare the section name used.
  /// </summary>
  public const string AppSettingsSection = "providers:blazot";

  /// <summary>
  /// Blazot issued API Key.
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;

  /// <summary>
  /// Blazot issued Secret Auth Key. This is only needed when making a request to get the access token at the auth endpoint.
  /// </summary>
  public string SecretAuthKey { get; set; } = string.Empty;

  /// <summary>
  /// The number of seconds in the rate limit window.
  /// </summary>
  public int WindowSeconds { get; set; }

  /// <summary>
  /// The number of requests the account allows within the window.
  /// </summary>
  public int WindowRequests { get; set; }
}
