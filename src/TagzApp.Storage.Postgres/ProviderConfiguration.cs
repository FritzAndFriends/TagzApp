﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TagzApp.Storage.Postgres;

public class ProviderConfiguration
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public JsonDocument? ConfigurationSettings { get; set; }
	public bool Activated { get; set; }

	public void Dispose() => ConfigurationSettings?.Dispose();
}
