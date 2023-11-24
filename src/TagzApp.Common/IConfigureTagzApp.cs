// Ignore Spelling: Tagz

namespace TagzApp.Common;


public interface IConfigureTagzApp
{

	Task InitializeConfiguration(string providerName, string configurationString);

	Task<T> GetConfigurationById<T>(string id) where T : new();

	Task SetConfigurationById<T>(string id, T value);

}

public class EmptyConfigureTagzApp : IConfigureTagzApp
{

	public static IConfigureTagzApp Instance = new EmptyConfigureTagzApp();

	public Task<T> GetConfigurationById<T>(string id) where T : new()
	{
		return Task.FromResult(new T());
	}

	public Task InitializeConfiguration(string providerName, string configurationString)
	{
		return Task.CompletedTask;
	}

	public Task SetConfigurationById<T>(string id, T value)
	{
		return Task.CompletedTask;
	}
}
