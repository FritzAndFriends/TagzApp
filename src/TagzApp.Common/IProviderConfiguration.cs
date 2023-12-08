namespace TagzApp.Common;

/// <summary>
/// Tagging interface to build provider configuration forms from
/// </summary>
public interface IProviderConfiguration
{

	public string Name { get; }
	public string Description { get; }

	public bool Enabled { get; set; }
}
