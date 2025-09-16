// Ignore Spelling: Tagz

using Azure.Security.KeyVault.Secrets;
using AzureKeyVaultEmulator.Aspire.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TagzApp.Common;

namespace TagzApp.Configuration.AzureKeyVault;

/// <summary>
/// Extension methods for registering Azure Key Vault configuration services
/// </summary>
public static class KeyVaultExtensions
{

	private static AzureKeyVaultConfigurationOptions _Options = new();

	public static IServiceCollection SetKeyVaultOptions(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
	{

		var vaultUri = configuration.GetConnectionString("vault");

		if (isDevelopment)
			services.AddAzureKeyVaultEmulator(vaultUri, secrets: true, certificates: true, keys: true);
		else
			services.AddScoped<SecretClient>(_ => 			{
				var client = new SecretClient(new Uri(vaultUri), new Azure.Identity.DefaultAzureCredential());
				return client;
			});


		return services;
	}


	/// <summary>
	/// Adds Azure Key Vault configuration provider to the service collection
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configuration">The configuration instance</param>
	/// <param name="configurationSection">The configuration section name (default: "AzureKeyVault")</param>
	/// <returns>The service collection for chaining</returns>
	public static IConfigureTagzApp AddAzureKeyVaultConfiguration(
			IServiceProvider services,
			IConfiguration configuration
			)
	{

		var logger = services.GetRequiredService<ILogger<AzureKeyVaultConfigureTagzApp>>();
		var secretClient = services.GetRequiredService<SecretClient>();

		return new AzureKeyVaultConfigureTagzApp(secretClient, logger);
	}

}
