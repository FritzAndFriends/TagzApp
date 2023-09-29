namespace TagzApp.Common.Models;

public class ProviderConfiguration
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public Dictionary<string, string>? ConfigurationSettings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	public bool Activated { get; set; }

}
