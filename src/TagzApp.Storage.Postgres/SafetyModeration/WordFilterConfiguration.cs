namespace TagzApp.Storage.Postgres.SafetyModeration;

public class WordFilterConfiguration
{

	public const string ConfigurationKey = "wordfilter";

	public bool Enabled { get; set; } = false;

	public string[] BlockedWords { get; set; } = Array.Empty<string>();

}