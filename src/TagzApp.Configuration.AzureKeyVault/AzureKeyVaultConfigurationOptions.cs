// Ignore Spelling: Tagz

namespace TagzApp.Configuration.AzureKeyVault;

/// <summary>
/// Configuration options for Azure Key Vault integration
/// </summary>
public class AzureKeyVaultConfigurationOptions
{
	/// <summary>
	/// The Azure Key Vault URI (e.g., https://your-keyvault.vault.azure.net/)
	/// </summary>
	public string? KeyVaultUri { get; set; }

	/// <summary>
	/// Optional: Azure tenant ID for authentication
	/// </summary>
	public string? TenantId { get; set; }

	/// <summary>
	/// Optional: Client ID for service principal authentication
	/// </summary>
	public string? ClientId { get; set; }

	/// <summary>
	/// Optional: Client secret for service principal authentication
	/// </summary>
	public string? ClientSecret { get; set; }

	/// <summary>
	/// Prefix for configuration keys in Key Vault (default: "TagzApp-")
	/// </summary>
	public string KeyPrefix { get; set; } = "TagzApp-";

	/// <summary>
	/// Cache TTL for configuration values in minutes (default: 60 minutes)
	/// </summary>
	public int CacheTtlMinutes { get; set; } = 60;

	/// <summary>
	/// Whether to use managed identity for authentication (default: true)
	/// </summary>
	public bool UseManagedIdentity { get; set; } = true;

	/// <summary>
	/// Validate that required configuration is present
	/// </summary>
	public void Validate()
	{
		if (string.IsNullOrWhiteSpace(KeyVaultUri))
		{
			throw new ArgumentException("KeyVaultUri is required", nameof(KeyVaultUri));
		}

		if (!Uri.TryCreate(KeyVaultUri, UriKind.Absolute, out _))
		{
			throw new ArgumentException("KeyVaultUri must be a valid URI", nameof(KeyVaultUri));
		}

		if (!UseManagedIdentity)
		{
			if (string.IsNullOrWhiteSpace(ClientId))
			{
				throw new ArgumentException("ClientId is required when not using managed identity", nameof(ClientId));
			}

			if (string.IsNullOrWhiteSpace(ClientSecret))
			{
				throw new ArgumentException("ClientSecret is required when not using managed identity", nameof(ClientSecret));
			}
		}
	}
}
