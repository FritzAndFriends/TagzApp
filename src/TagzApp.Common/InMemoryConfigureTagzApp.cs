// Ignore Spelling: Tagz

using System.Text.Json;

namespace TagzApp.Common;

public class InMemoryConfigureTagzApp : IConfigureTagzApp
{

	private static Dictionary<string, string> _Db = new();

	public Task<T> GetConfigurationById<T>(string id) where T : new()
	{

		if (!_Db.ContainsKey(id)) { return Task.FromResult(new T()); }

		return Task.FromResult(JsonSerializer.Deserialize<T>(_Db[id]))!;

	}

	public Task<string> GetConfigurationStringById(string id)
	{
		if (!_Db.ContainsKey(id)) { return Task.FromResult(string.Empty); }

		return Task.FromResult(_Db[id]);
	}

	public Task InitializeConfiguration(string providerName, string configurationString)
	{
		return Task.CompletedTask;
	}

	public Task SetConfigurationById<T>(string id, T value)
	{
		var outValue = JsonSerializer.Serialize(value);
		_Db[id] = outValue;
		return Task.CompletedTask;
	}
}
