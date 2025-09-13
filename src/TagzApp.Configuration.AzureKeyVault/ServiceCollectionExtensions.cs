// Ignore Spelling: Tagz

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TagzApp.Common;

namespace TagzApp.Configuration.AzureKeyVault;

/// <summary>
/// Extension methods for registering Azure Key Vault configuration services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Key Vault configuration provider to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="configurationSection">The configuration section name (default: "AzureKeyVault")</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureKeyVaultConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSection = "AzureKeyVault")
    {
        // Configure options from configuration
        services.Configure<AzureKeyVaultConfigurationOptions>(
            configuration.GetSection(configurationSection));

        // Register the configuration provider
        services.TryAddSingleton<IConfigureTagzApp, AzureKeyVaultConfigureTagzApp>();

        return services;
    }

    /// <summary>
    /// Adds Azure Key Vault configuration provider to the service collection with explicit options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the Azure Key Vault options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureKeyVaultConfiguration(
        this IServiceCollection services,
        Action<AzureKeyVaultConfigurationOptions> configureOptions)
    {
        // Configure options via action
        services.Configure(configureOptions);

        // Register the configuration provider
        services.TryAddSingleton<IConfigureTagzApp, AzureKeyVaultConfigureTagzApp>();

        return services;
    }

    /// <summary>
    /// Adds Azure Key Vault configuration provider to the service collection with managed identity
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="keyVaultUri">The Azure Key Vault URI</param>
    /// <param name="keyPrefix">Optional key prefix (default: "TagzApp-")</param>
    /// <param name="cacheTtlMinutes">Optional cache TTL in minutes (default: 60)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureKeyVaultConfigurationWithManagedIdentity(
        this IServiceCollection services,
        string keyVaultUri,
        string keyPrefix = "TagzApp-",
        int cacheTtlMinutes = 60)
    {
        services.Configure<AzureKeyVaultConfigurationOptions>(options =>
        {
            options.KeyVaultUri = keyVaultUri;
            options.KeyPrefix = keyPrefix;
            options.CacheTtlMinutes = cacheTtlMinutes;
            options.UseManagedIdentity = true;
        });

        services.TryAddSingleton<IConfigureTagzApp, AzureKeyVaultConfigureTagzApp>();

        return services;
    }

    /// <summary>
    /// Adds Azure Key Vault configuration provider to the service collection with service principal authentication
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="keyVaultUri">The Azure Key Vault URI</param>
    /// <param name="tenantId">The Azure tenant ID</param>
    /// <param name="clientId">The service principal client ID</param>
    /// <param name="clientSecret">The service principal client secret</param>
    /// <param name="keyPrefix">Optional key prefix (default: "TagzApp-")</param>
    /// <param name="cacheTtlMinutes">Optional cache TTL in minutes (default: 60)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureKeyVaultConfigurationWithServicePrincipal(
        this IServiceCollection services,
        string keyVaultUri,
        string tenantId,
        string clientId,
        string clientSecret,
        string keyPrefix = "TagzApp-",
        int cacheTtlMinutes = 60)
    {
        services.Configure<AzureKeyVaultConfigurationOptions>(options =>
        {
            options.KeyVaultUri = keyVaultUri;
            options.TenantId = tenantId;
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.KeyPrefix = keyPrefix;
            options.CacheTtlMinutes = cacheTtlMinutes;
            options.UseManagedIdentity = false;
        });

        services.TryAddSingleton<IConfigureTagzApp, AzureKeyVaultConfigureTagzApp>();

        return services;
    }
}

/// <summary>
/// Extension methods for configuring Azure Key Vault with ASP.NET Core configuration
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds Azure Key Vault as a configuration source for TagzApp settings
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="keyVaultUri">The Azure Key Vault URI</param>
    /// <param name="keyPrefix">The key prefix to filter secrets (default: "TagzApp-")</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddTagzAppAzureKeyVault(
        this IConfigurationBuilder builder,
        string keyVaultUri,
        string keyPrefix = "TagzApp-")
    {
        var keyVaultUriObj = new Uri(keyVaultUri);

        // Add Azure Key Vault configuration source
        builder.AddAzureKeyVault(keyVaultUriObj, new Azure.Identity.DefaultAzureCredential());

        return builder;
    }

    /// <summary>
    /// Adds Azure Key Vault as a configuration source with service principal authentication
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="keyVaultUri">The Azure Key Vault URI</param>
    /// <param name="tenantId">The Azure tenant ID</param>
    /// <param name="clientId">The service principal client ID</param>
    /// <param name="clientSecret">The service principal client secret</param>
    /// <param name="keyPrefix">The key prefix to filter secrets (default: "TagzApp-")</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddTagzAppAzureKeyVault(
        this IConfigurationBuilder builder,
        string keyVaultUri,
        string tenantId,
        string clientId,
        string clientSecret,
        string keyPrefix = "TagzApp-")
    {
        var keyVaultUriObj = new Uri(keyVaultUri);
        var credential = new Azure.Identity.ClientSecretCredential(tenantId, clientId, clientSecret);

        // Add Azure Key Vault configuration source
        builder.AddAzureKeyVault(keyVaultUriObj, credential);

        return builder;
    }
}
