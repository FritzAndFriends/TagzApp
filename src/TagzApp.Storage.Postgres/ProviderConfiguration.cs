using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagzApp.Storage.Postgres;

public class ProviderConfiguration
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public bool Activated { get; set; }

	// Supports up to 10 configuration values per provider
	public string? ConfigValue1 { get; set; }
	public string? ConfigValue2 { get; set; }
	public string? ConfigValue3 { get; set; }
	public string? ConfigValue4 { get; set; }
	public string? ConfigValue5 { get; set; }
	public string? ConfigValue6 { get; set; }
	public string? ConfigValue7 { get; set; }
	public string? ConfigValue8 { get; set; }
	public string? ConfigValue9 { get; set; }
	public string? ConfigValue10 { get; set; }
}