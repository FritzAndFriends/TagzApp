using System.Text.Json;

namespace TagzApp.Common.Models;

public class ProviderConfiguration
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public JsonDocument? ConfigurationSettings { get; set; }
	public bool Activated { get; set; }

	public void Dispose() => ConfigurationSettings?.Dispose();
}
