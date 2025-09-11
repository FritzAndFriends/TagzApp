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

/// <summary>
/// Base class for provider configurations that handles common configuration operations
/// </summary>
public abstract class BaseProviderConfiguration<T> : IProviderConfiguration where T : BaseProviderConfiguration<T>, new()
{
	/// <summary>
	/// The configuration key used to store this configuration in the TagzApp configuration system
	/// </summary>
	protected abstract string ConfigurationKey { get; }

	public abstract string Name { get; }
	public abstract string Description { get; }
	public abstract bool Enabled { get; set; }
	public abstract string[] Keys { get; }
	public abstract string GetConfigurationByKey(string key);
	public abstract void SetConfigurationByKey(string key, string value);

	/// <summary>
	/// Loads configuration from the TagzApp configuration system and updates this instance
	/// </summary>
	/// <param name="configureTagzApp">The configuration provider</param>
	public async Task LoadFromConfigurationAsync(IConfigureTagzApp configureTagzApp)
	{
		var storedConfig = await configureTagzApp.GetConfigurationById<T>(ConfigurationKey);
		UpdateFromConfiguration(storedConfig);
	}

	/// <summary>
	/// Saves this configuration to the TagzApp configuration system
	/// </summary>
	/// <param name="configureTagzApp">The configuration provider</param>
	public async Task SaveToConfigurationAsync(IConfigureTagzApp configureTagzApp)
	{
		await configureTagzApp.SetConfigurationById(ConfigurationKey, (T)this);
	}

	/// <summary>
	/// Updates this instance with values from another configuration instance
	/// </summary>
	/// <param name="source">The source configuration to copy from</param>
	protected abstract void UpdateFromConfiguration(T source);

	/// <summary>
	/// Creates a new instance loaded from the configuration system
	/// </summary>
	/// <param name="configureTagzApp">The configuration provider</param>
	/// <returns>A new configuration instance loaded from storage</returns>
	public static async Task<TConfig> CreateFromConfigurationAsync<TConfig>(IConfigureTagzApp configureTagzApp) where TConfig : BaseProviderConfiguration<TConfig>, new()
	{
		var instance = new TConfig();
		await instance.LoadFromConfigurationAsync(configureTagzApp);
		return instance;
	}

	/// <summary>
	/// Gets the configuration key used by this configuration type
	/// </summary>
	protected string GetConfigurationKey() => ConfigurationKey;
}
