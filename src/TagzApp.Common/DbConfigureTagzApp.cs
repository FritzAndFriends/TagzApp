// Ignore Spelling: Tagz

using Dapper;
using Microsoft.Data.Sqlite;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace TagzApp.Common;

public class DbConfigureTagzApp : IConfigureTagzApp, IDisposable
{

	private bool _DisposedValue;

	public static IEnumerable<string> SupportedDbs = ["postgres", "sqlite"];
	private static string _ProviderName;
	private static string _ConnectionString;

	public async Task<T> GetConfigurationById<T>(string id) where T : new()
	{

		using var conn = GetConnection();
		var outValue = await conn.QuerySingleOrDefaultAsync<string>("select value from SystemConfiguration WHERE id=@id ", new { id = id });

		if (string.IsNullOrEmpty(outValue)) { return new T(); }

		return JsonSerializer.Deserialize<T>(outValue);

	}

	public async Task<string> GetConfigurationStringById(string id)
	{

		using var conn = GetConnection();
		var outValue = await conn.QuerySingleOrDefaultAsync<string>("select value from SystemConfiguration WHERE id=@id ", new { id = id });

		if (string.IsNullOrEmpty(outValue)) { return string.Empty; }

		return JsonSerializer.Deserialize<string>(outValue);
	}


	public Task InitializeConfiguration(string providerName, string connectionString)
	{

		_ProviderName = providerName;
		_ConnectionString = connectionString;

		using var conn = GetConnection(providerName, connectionString);

		try
		{
			conn.Open();
		}
		catch (NpgsqlException ex)
		{
			throw new Exception($"Unable to connect to configuration Postgres database: {ex.Message}", ex);
		}
		catch (Exception ex)
		{
			throw new Exception("Unable to connect to configuration database", ex);
		}

		CreateConfigTable(conn, providerName);

		return Task.CompletedTask;


	}

	private void CreateConfigTable(IDbConnection conn, string providerName)
	{

		// Create the SystemConfiguration table for postgres and sqlite
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "CREATE TABLE IF NOT EXISTS SystemConfiguration (Id TEXT PRIMARY KEY, Value TEXT)";
		cmd.ExecuteNonQuery();

	}

	private IDbConnection GetConnection() => GetConnection(_ProviderName, _ConnectionString);

	private static IDbConnection GetConnection(string providerName, string connectionString)
	{

		return providerName.ToLowerInvariant() switch
		{
			"sqlite" => new SqliteConnection(connectionString),
			"postgres" => new Npgsql.NpgsqlConnection(connectionString),
			_ => throw new NotImplementedException()
		};

	}

	public async Task SetConfigurationById<T>(string id, T value)
	{

		var outValue = JsonSerializer.Serialize(value);

		using var conn = GetConnection();
		var exists = await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM SystemConfiguration WHERE id = @id", new { id = id });
		int result = 0;

		if (exists == 0)
		{
			result = await conn.ExecuteAsync("INSERT INTO SystemConfiguration (Id, Value) VALUES (@id, @value)", new { id = id, value = outValue });
		}
		else
		{
			result = await conn.ExecuteAsync("UPDATE SystemConfiguration SET Value = @value WHERE Id = @id", new { id = id, value = outValue });
		}

		if (result == 0)
		{
			throw new Exception("Unable to set configuration");
		}

	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}


			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	~DbConfigureTagzApp()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	void IDisposable.Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

}
