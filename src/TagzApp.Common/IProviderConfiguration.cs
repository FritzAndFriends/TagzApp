namespace TagzApp.Common;

/// <summary>
/// Tagging interface to build provider configuration forms from
/// </summary>
public interface IProviderConfiguration
{

	string Name { get; }
	string Description { get; }
	bool Enabled { get; set; }

	string[] Keys { get; }
	string GetConfigurationByKey(string key);
	void SetConfigurationByKey(string key, string value);

}
