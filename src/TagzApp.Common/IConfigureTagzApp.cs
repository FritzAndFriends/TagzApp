// Ignore Spelling: Tagz

namespace TagzApp.Common;


public interface IConfigureTagzApp
{

	Task InitializeConfiguration(string providerName, string configurationString);

	Task<T> GetConfigurationById<T>(string id) where T : new();

	Task<string> GetConfigurationStringById(string id);

	Task SetConfigurationById<T>(string id, T value);

}

public class EmptyConfigureTagzApp : IConfigureTagzApp
{

	public static EmptyConfigureTagzApp Instance = new();

	public string Message { get; set; } = "";

	public Task<T> GetConfigurationById<T>(string id) where T : new()
	{
		return Task.FromResult(new T());
	}

	public Task<string> GetConfigurationStringById(string id) => Task.FromResult(string.Empty);

	public Task InitializeConfiguration(string providerName, string configurationString)
	{
		return Task.CompletedTask;
	}

	public Task SetConfigurationById<T>(string id, T value)
	{
		return Task.CompletedTask;
	}
}
