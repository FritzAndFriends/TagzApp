// Ignore Spelling: Tagz

namespace TagzApp.Common;

public class ConnectionSettings
{

	public const string ConfigurationKey = "ConnectionSettings";

	public string SecurityProvider { get; set; } = string.Empty;

	public string SecurityConnectionString { get; set; } = string.Empty;

	public string ContentProvider { get; set; } = string.Empty;

	public string ContentConnectionString { get; set; } = string.Empty;


}
