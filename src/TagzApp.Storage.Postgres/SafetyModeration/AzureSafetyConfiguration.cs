namespace TagzApp.Storage.Postgres.SafetyModeration;

public class AzureSafetyConfiguration
{

	public const string ConfigurationKey = "azuresafety";

	public bool Enabled { get; set; } = false;

	public string Key { get; set; } = string.Empty;

	public string Endpoint { get; set; } = string.Empty;

}
