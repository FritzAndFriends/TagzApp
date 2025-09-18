// Ignore Spelling: Tagz

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using TagzApp.Common;

namespace TagzApp.Configuration.AzureKeyVault;

/// <summary>
/// Azure Key Vault implementation of IConfigureTagzApp that securely stores configuration data in Azure Key Vault
/// </summary>
public class AzureKeyVaultConfigureTagzApp : IConfigureTagzApp, IDisposable
{
	private readonly AzureKeyVaultConfigurationOptions _options = new();
	private readonly ILogger<AzureKeyVaultConfigureTagzApp> _logger;
	private readonly SecretClient _secretClient;
	private readonly ConcurrentDictionary<string, CachedSecret> _cache;
	private readonly Timer _cacheCleanupTimer;
	private bool _disposed;

	public AzureKeyVaultConfigureTagzApp(
			SecretClient secretClient,
			ILogger<AzureKeyVaultConfigureTagzApp> logger)
	{
		ArgumentNullException.ThrowIfNull(secretClient);
		ArgumentNullException.ThrowIfNull(logger);

		_logger = logger;

		// Initialize cache
		_cache = new ConcurrentDictionary<string, CachedSecret>();

		// Create Azure Key Vault client
		_secretClient = secretClient; //CreateSecretClient();

		// Setup cache cleanup timer (runs every 5 minutes)
		_cacheCleanupTimer = new Timer(CleanupExpiredCache, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

		_logger.LogInformation($"Azure Key Vault configuration provider initialized with vault: {secretClient.VaultUri}");
	}

	public async Task InitializeConfiguration(string providerName, string configurationString)
	{
		// Azure Key Vault doesn't need traditional initialization like a database
		// The configuration is passed through options instead

		await SetConfigurationById<ConnectionSettings>(ConnectionSettings.ConfigurationKey, new ConnectionSettings
		{
			ContentProvider = providerName,
			ContentConnectionString = configurationString,
			SecurityProvider = providerName,
		});


		_logger.LogInformation("Azure Key Vault configuration provider initialized for provider: {ProviderName}",
				providerName);

	}

	public async Task<T> GetConfigurationById<T>(string id) where T : new()
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);

		try
		{
			var configValue = await GetConfigurationStringById(id);

			if (string.IsNullOrEmpty(configValue))
			{
				_logger.LogDebug("Configuration not found for id: {Id}, returning default value", id);
				return new T();
			}

			var result = JsonSerializer.Deserialize<T>(configValue);
			_logger.LogDebug("Successfully retrieved and deserialized configuration for id: {Id}", id);
			return result ?? new T();
		}
		catch (JsonException ex)
		{
			_logger.LogError(ex, "Failed to deserialize configuration for id: {Id}", id);
			return new T();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to retrieve configuration for id: {Id}", id);
			return new T();
		}
	}

	public async Task<string> GetConfigurationStringById(string id)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);

		var secretName = GetSecretName(id);

		// Check cache first
		if (_cache.TryGetValue(secretName, out var cachedSecret) && !cachedSecret.IsExpired)
		{
			_logger.LogDebug("Retrieved configuration from cache for id: {Id}", id);
			return cachedSecret.Value;
		}

		try
		{
			var response = await _secretClient.GetSecretAsync(secretName);
			var secretValue = response.Value.Value;

			// Cache the result
			var cacheEntry = new CachedSecret(secretValue, DateTime.UtcNow.AddMinutes(_options.CacheTtlMinutes));
			_cache.AddOrUpdate(secretName, cacheEntry, (_, _) => cacheEntry);

			_logger.LogDebug("Successfully retrieved configuration from Azure Key Vault for id: {Id}", id);
			return secretValue;
		}
		catch (Azure.RequestFailedException ex) when (ex.Status == 404)
		{
			_logger.LogDebug("Configuration not found in Azure Key Vault for id: {Id}", id);
			return string.Empty;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to retrieve configuration from Azure Key Vault for id: {Id}", id);
			throw new InvalidOperationException($"Failed to retrieve configuration for id: {id}", ex);
		}
	}

	public async Task SetConfigurationById<T>(string id, T value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		ArgumentNullException.ThrowIfNull(value);

		var secretName = GetSecretName(id);
		var serializedValue = JsonSerializer.Serialize(value);

		try
		{
			await _secretClient.SetSecretAsync(secretName, serializedValue);

			// Update cache
			var cacheEntry = new CachedSecret(serializedValue, DateTime.UtcNow.AddMinutes(_options.CacheTtlMinutes));
			_cache.AddOrUpdate(secretName, cacheEntry, (_, _) => cacheEntry);

			_logger.LogInformation("Successfully stored configuration in Azure Key Vault for id: {Id}", id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to store configuration in Azure Key Vault for id: {Id}", id);
			throw new InvalidOperationException($"Failed to store configuration for id: {id}", ex);
		}
	}

	/// <summary>
	/// Removes a configuration value from Azure Key Vault
	/// </summary>
	/// <param name="id">The configuration identifier</param>
	public async Task DeleteConfigurationById(string id)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);

		var secretName = GetSecretName(id);

		try
		{
			// Start the delete operation
			var deleteOperation = await _secretClient.StartDeleteSecretAsync(secretName);

			// Remove from cache
			_cache.TryRemove(secretName, out _);

			_logger.LogInformation("Successfully initiated deletion of configuration in Azure Key Vault for id: {Id}", id);

			// Optionally wait for completion (uncomment if needed)
			// await deleteOperation.WaitForCompletionAsync();
		}
		catch (Azure.RequestFailedException ex) when (ex.Status == 404)
		{
			_logger.LogDebug("Configuration not found in Azure Key Vault for deletion, id: {Id}", id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to delete configuration from Azure Key Vault for id: {Id}", id);
			throw new InvalidOperationException($"Failed to delete configuration for id: {id}", ex);
		}
	}

	/// <summary>
	/// Lists all configuration keys with the configured prefix
	/// </summary>
	/// <returns>Collection of configuration identifiers</returns>
	public async Task<IEnumerable<string>> ListConfigurationIds()
	{
		try
		{
			var secretIds = new List<string>();

			await foreach (var secretProperties in _secretClient.GetPropertiesOfSecretsAsync())
			{
				if (secretProperties.Name.StartsWith(_options.KeyPrefix, StringComparison.OrdinalIgnoreCase))
				{
					var configId = secretProperties.Name[_options.KeyPrefix.Length..];
					secretIds.Add(configId);
				}
			}

			_logger.LogDebug("Listed {Count} configuration ids from Azure Key Vault", secretIds.Count);
			return secretIds;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to list configuration ids from Azure Key Vault");
			throw new InvalidOperationException("Failed to list configuration ids", ex);
		}
	}

	private SecretClient CreateSecretClient()
	{
		var keyVaultUri = new Uri(_options.KeyVaultUri!);

		// Allows "localhost" to be used instead of "<vault-name>.vault.azure.net" as the vaultUri
		var options = new SecretClientOptions {
			DisableChallengeResourceVerification = keyVaultUri.Host.Contains("localhost", StringComparison.InvariantCultureIgnoreCase)
		};


		if (_options.UseManagedIdentity)
		{
			_logger.LogInformation("Using managed identity for Azure Key Vault authentication");
			return new SecretClient(keyVaultUri, new DefaultAzureCredential(), options);
		}
		else
		{
			_logger.LogInformation("Using client credentials for Azure Key Vault authentication");
			var credential = new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret);
			return new SecretClient(keyVaultUri, credential, options);
		}
	}

	private string GetSecretName(string id)
	{
		// Azure Key Vault secret names must be alphanumeric and dashes only
		var sanitizedId = SanitizeSecretName(id);
		return $"{_options.KeyPrefix}{sanitizedId}";
	}

	private static string SanitizeSecretName(string input)
	{
		// Azure Key Vault allows alphanumeric characters and dashes
		// Replace any invalid characters with dashes
		var chars = input.ToCharArray();
		for (int i = 0; i < chars.Length; i++)
		{
			if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '-')
			{
				chars[i] = '-';
			}
		}

		var result = new string(chars);

		// Ensure it doesn't start or end with a dash
		result = result.Trim('-');

		// Ensure it's not empty and not too long (max 127 characters)
		if (string.IsNullOrEmpty(result))
		{
			result = "config";
		}

		if (result.Length > 100) // Leave room for prefix
		{
			result = result[..100];
		}

		return result;
	}

	private void CleanupExpiredCache(object? state)
	{
		try
		{
			var expiredKeys = _cache
					.Where(kvp => kvp.Value.IsExpired)
					.Select(kvp => kvp.Key)
					.ToList();

			foreach (var key in expiredKeys)
			{
				_cache.TryRemove(key, out _);
			}

			if (expiredKeys.Count > 0)
			{
				_logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Error during cache cleanup");
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_cacheCleanupTimer?.Dispose();
			_cache.Clear();
			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private record CachedSecret(string Value, DateTime ExpiresAt)
	{
		public bool IsExpired => DateTime.UtcNow > ExpiresAt;
	}
}
