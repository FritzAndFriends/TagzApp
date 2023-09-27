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
	[Column(TypeName = "jsonb")]
	public Dictionary<string, string>? ConfigurationSettings { get; set; }
	public bool Activated { get; set; }
}
