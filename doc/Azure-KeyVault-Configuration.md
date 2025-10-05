# Azure Key Vault Configuration Provider

The TagzApp.Configuration.AzureKeyVault library provides a secure, cloud-based configuration storage solution for TagzApp using Azure Key Vault. This implementation stores configuration data as secrets in Azure Key Vault, providing enterprise-grade security, access control, and audit capabilities.

## Features

- **Secure Storage**: Configuration data is stored as secrets in Azure Key Vault
- **Multiple Authentication Methods**: Supports both Managed Identity and Service Principal authentication
- **Intelligent Caching**: Built-in caching with configurable TTL to reduce API calls
- **Secret Name Sanitization**: Automatically handles invalid characters in configuration keys
- **Comprehensive Logging**: Detailed logging for troubleshooting and monitoring
- **Configuration Management**: Additional methods for listing and deleting configurations
- **Full IConfigureTagzApp Implementation**: Drop-in replacement for database configuration

## Quick Start

### 1. Install the Package

Add the TagzApp.Configuration.AzureKeyVault project reference to your application:

```xml
<ProjectReference Include="TagzApp.Configuration.AzureKeyVault\TagzApp.Configuration.AzureKeyVault.csproj" />
```

### 2. Configure Azure Key Vault

Add Azure Key Vault configuration to your `appsettings.json`:

```json
{
  "AzureKeyVault": {
    "KeyVaultUri": "https://your-keyvault.vault.azure.net/",
    "UseManagedIdentity": true,
    "KeyPrefix": "TagzApp-",
    "CacheTtlMinutes": 60
  }
}
```

### 3. Register the Service

In your `Program.cs` or startup configuration:

```csharp
using TagzApp.Configuration.AzureKeyVault;

// Option 1: Using configuration section
services.AddAzureKeyVaultConfiguration(configuration);

// Option 2: Using explicit configuration
services.AddAzureKeyVaultConfigurationWithManagedIdentity(
    "https://your-keyvault.vault.azure.net/");

// Option 3: Using service principal
services.AddAzureKeyVaultConfigurationWithServicePrincipal(
    keyVaultUri: "https://your-keyvault.vault.azure.net/",
    tenantId: "your-tenant-id",
    clientId: "your-client-id",
    clientSecret: "your-client-secret");
```

## Configuration Options

### AzureKeyVaultConfigurationOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `KeyVaultUri` | string | *required* | The Azure Key Vault URI (e.g., https://your-keyvault.vault.azure.net/) |
| `UseManagedIdentity` | bool | `true` | Whether to use Managed Identity for authentication |
| `TenantId` | string? | `null` | Azure tenant ID (required for service principal auth) |
| `ClientId` | string? | `null` | Service principal client ID (required for service principal auth) |
| `ClientSecret` | string? | `null` | Service principal client secret (required for service principal auth) |
| `KeyPrefix` | string | `"TagzApp-"` | Prefix for configuration keys in Key Vault |
| `CacheTtlMinutes` | int | `60` | Cache TTL for configuration values in minutes |

### Configuration Validation

The configuration options are automatically validated on startup:

- `KeyVaultUri` must be a valid URI
- When `UseManagedIdentity` is `false`, `ClientId` and `ClientSecret` are required
- All required fields must be non-empty

## Authentication Methods

### Managed Identity (Recommended)

Managed Identity is the recommended authentication method for Azure-hosted applications:

```json
{
  "AzureKeyVault": {
    "KeyVaultUri": "https://your-keyvault.vault.azure.net/",
    "UseManagedIdentity": true
  }
}
```

**Setup Requirements:**
1. Enable Managed Identity on your Azure resource (App Service, Container Instance, etc.)
2. Grant the Managed Identity appropriate permissions on the Key Vault
3. Required Key Vault permissions: Get, Set, List, Delete secrets

### Service Principal

For non-Azure environments or when Managed Identity is not available:

```json
{
  "AzureKeyVault": {
    "KeyVaultUri": "https://your-keyvault.vault.azure.net/",
    "UseManagedIdentity": false,
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

**Setup Requirements:**
1. Create an Azure AD application/service principal
2. Create a client secret for the application
3. Grant the service principal appropriate permissions on the Key Vault

## Key Vault Permissions

The Azure Key Vault configuration provider requires the following permissions:

### Secret Permissions

- **Get**: Read configuration values
- **Set**: Write configuration values  
- **List**: List all configuration keys
- **Delete**: Remove configuration values (optional, for cleanup)

### Access Policy Example

```bash
# Using Azure CLI
az keyvault set-policy \
  --name your-keyvault \
  --object-id <managed-identity-or-service-principal-id> \
  --secret-permissions get set list delete
```

### RBAC Role Example

```bash
# Using Azure CLI with RBAC
az role assignment create \
  --assignee <managed-identity-or-service-principal-id> \
  --role "Key Vault Secrets Officer" \
  --scope /subscriptions/<subscription-id>/resourceGroups/<rg>/providers/Microsoft.KeyVault/vaults/<keyvault-name>
```

## Usage Examples

### Basic Configuration Operations

```csharp
public class ExampleService
{
    private readonly IConfigureTagzApp _configProvider;

    public ExampleService(IConfigureTagzApp configProvider)
    {
        _configProvider = configProvider;
    }

    public async Task ConfigureProvider()
    {
        // Set a configuration
        var config = new TwitterConfig 
        { 
            ApiKey = "your-api-key",
            ApiSecret = "your-api-secret"
        };
        await _configProvider.SetConfigurationById("twitter-config", config);

        // Get a configuration
        var retrievedConfig = await _configProvider.GetConfigurationById<TwitterConfig>("twitter-config");

        // Get as string
        var configString = await _configProvider.GetConfigurationStringById("twitter-config");
    }
}
```

### Advanced Operations (Azure Key Vault Specific)

```csharp
public class AdvancedConfigService
{
    private readonly AzureKeyVaultConfigureTagzApp _azureConfigProvider;

    public AdvancedConfigService(IConfigureTagzApp configProvider)
    {
        // Cast to access Azure Key Vault specific methods
        _azureConfigProvider = (AzureKeyVaultConfigureTagzApp)configProvider;
    }

    public async Task ManageConfigurations()
    {
        // List all configuration IDs
        var configIds = await _azureConfigProvider.ListConfigurationIds();

        // Delete a configuration
        await _azureConfigProvider.DeleteConfigurationById("old-config");
    }
}
```

## Key Management

### Secret Naming Convention

Configuration keys are automatically sanitized and prefixed:

- **Input key**: `provider.twitter.config`
- **Key Vault secret name**: `TagzApp-provider-twitter-config`

### Secret Name Sanitization

- Invalid characters are replaced with dashes (`-`)
- Leading/trailing dashes are removed
- Names are truncated to 100 characters (leaving room for prefix)
- Empty names default to `"config"`

### Examples

| Original ID | Sanitized Name | Full Secret Name |
|-------------|----------------|------------------|
| `twitter.api.config` | `twitter-api-config` | `TagzApp-twitter-api-config` |
| `mastodon@config!` | `mastodon-config` | `TagzApp-mastodon-config` |
| `provider/youtube/settings` | `provider-youtube-settings` | `TagzApp-provider-youtube-settings` |

## Caching

The provider includes intelligent caching to reduce Azure Key Vault API calls:

- **Cache Duration**: Configurable TTL (default: 60 minutes)
- **Cache Key**: Based on the full secret name
- **Cache Invalidation**: Automatic cleanup of expired entries
- **Cache Updates**: Cache is updated on both read and write operations

### Cache Behavior

1. **Read Operation**: Check cache first, fallback to Key Vault if expired/missing
2. **Write Operation**: Update Key Vault and refresh cache entry
3. **Delete Operation**: Remove from both Key Vault and cache
4. **Background Cleanup**: Expired cache entries are cleaned up every 5 minutes

## Error Handling

### Common Scenarios

| Error | Description | Resolution |
|-------|-------------|------------|
| `ArgumentException: KeyVaultUri is required` | Missing or invalid Key Vault URI | Provide valid Key Vault URI in configuration |
| `ArgumentException: ClientId is required` | Service principal auth missing credentials | Provide ClientId and ClientSecret |
| `Azure.RequestFailedException: 403 Forbidden` | Insufficient permissions | Grant appropriate Key Vault permissions |
| `Azure.RequestFailedException: 404 Not Found` | Secret doesn't exist | Configuration returns default values (normal behavior) |
| `InvalidOperationException: Failed to retrieve configuration` | General Key Vault error | Check network connectivity and permissions |

### Exception Handling Strategy

```csharp
try
{
    var config = await configProvider.GetConfigurationById<MyConfig>("my-config");
}
catch (InvalidOperationException ex)
{
    // Log the error and use fallback configuration
    logger.LogError(ex, "Failed to retrieve configuration from Azure Key Vault");
    var fallbackConfig = new MyConfig(); // Use defaults
}
```

## Development vs Production

### Development Environment

```json
{
  "AzureKeyVault": {
    "KeyVaultUri": "https://dev-keyvault.vault.azure.net/",
    "UseManagedIdentity": false,
    "TenantId": "dev-tenant-id",
    "ClientId": "dev-client-id", 
    "ClientSecret": "dev-client-secret",
    "KeyPrefix": "TagzApp-Dev-",
    "CacheTtlMinutes": 5
  }
}
```

### Production Environment

```json
{
  "AzureKeyVault": {
    "KeyVaultUri": "https://prod-keyvault.vault.azure.net/",
    "UseManagedIdentity": true,
    "KeyPrefix": "TagzApp-",
    "CacheTtlMinutes": 60
  }
}
```

**Production Best Practices:**
- Use Managed Identity when possible
- Store sensitive configuration in environment variables or Azure App Configuration
- Use separate Key Vaults for different environments
- Enable Key Vault logging and monitoring
- Implement proper backup and disaster recovery

## Migration from Database Configuration

The Azure Key Vault provider is a drop-in replacement for the database configuration:

### Step 1: Export Existing Configuration

```csharp
// Export from database provider
var dbProvider = serviceProvider.GetService<DbConfigureTagzApp>();
var configIds = await GetAllConfigurationIds(); // Custom method needed

var configurations = new Dictionary<string, string>();
foreach (var id in configIds)
{
    var value = await dbProvider.GetConfigurationStringById(id);
    if (!string.IsNullOrEmpty(value))
    {
        configurations[id] = value;
    }
}
```

### Step 2: Import to Azure Key Vault

```csharp
// Import to Azure Key Vault provider
var azureProvider = serviceProvider.GetService<AzureKeyVaultConfigureTagzApp>();

foreach (var kvp in configurations)
{
    // The value is already JSON serialized from the database
    var deserializedValue = JsonSerializer.Deserialize<object>(kvp.Value);
    await azureProvider.SetConfigurationById(kvp.Key, deserializedValue);
}
```

### Step 3: Update Service Registration

```csharp
// Old registration
services.AddSingleton<IConfigureTagzApp, DbConfigureTagzApp>();

// New registration  
services.AddAzureKeyVaultConfiguration(configuration);
```

## Monitoring and Logging

### Application Logging

The provider logs important events:

```csharp
// Successful operations
logger.LogInformation("Successfully stored configuration in Azure Key Vault for id: {Id}", id);
logger.LogDebug("Retrieved configuration from cache for id: {Id}", id);

// Errors
logger.LogError(ex, "Failed to retrieve configuration from Azure Key Vault for id: {Id}", id);
logger.LogWarning(ex, "Error during cache cleanup");
```

### Azure Key Vault Monitoring

Enable Azure Key Vault diagnostic settings to monitor:

- Secret access patterns
- Authentication failures  
- Performance metrics
- Security events

### Application Insights Integration

```csharp
services.AddApplicationInsightsTelemetry();
services.AddAzureKeyVaultConfiguration(configuration);
```

This automatically provides:
- Dependency tracking for Key Vault calls
- Performance monitoring
- Exception tracking
- Custom metrics

## Security Considerations

### Data at Rest

- **Encryption**: All secrets are encrypted at rest in Azure Key Vault
- **Key Management**: Azure manages encryption keys automatically
- **Compliance**: Key Vault meets various compliance standards (SOC, PCI DSS, etc.)

### Data in Transit

- **TLS**: All communication uses TLS 1.2+
- **Certificate Validation**: Azure SDK handles certificate validation
- **No Plain Text**: Configuration data is never transmitted in plain text

### Access Control

- **Principle of Least Privilege**: Grant minimum required permissions
- **Audit Logging**: Enable Key Vault audit logs
- **Network Isolation**: Use private endpoints when possible
- **Conditional Access**: Apply Azure AD conditional access policies

### Best Practices

1. **Separate Key Vaults**: Use different Key Vaults for different environments
2. **Regular Rotation**: Implement secret rotation policies
3. **Monitoring**: Set up alerts for unusual access patterns
4. **Backup**: Implement Key Vault backup strategies
5. **Network Security**: Use private endpoints and firewall rules

## Troubleshooting

### Configuration Issues

**Problem**: Configuration not found
```
logger.LogDebug("Configuration not found in Azure Key Vault for id: {Id}", id);
```
**Solution**: Verify the configuration was set and the key name is correct

**Problem**: Authentication failures
```
Azure.RequestFailedException: 401 Unauthorized
```
**Solution**: Verify Managed Identity or service principal permissions

### Performance Issues

**Problem**: Slow configuration retrieval
**Solution**: 
- Increase cache TTL
- Verify network connectivity to Key Vault
- Consider using private endpoints

**Problem**: High Key Vault costs
**Solution**:
- Increase cache TTL to reduce API calls
- Review configuration access patterns
- Consider consolidating similar configurations

### Development Issues

**Problem**: Local development authentication
**Solution**: Use Azure CLI login or Visual Studio authentication:

```bash
az login
az account set --subscription "your-subscription-id"
```

## Performance Characteristics

### Latency

- **Cache Hit**: ~1ms (in-memory lookup)
- **Cache Miss**: ~50-200ms (Azure Key Vault API call)
- **Write Operations**: ~100-300ms (Azure Key Vault API call + cache update)

### Throughput

- **Azure Key Vault Limits**: ~2000 requests per 10 seconds per vault
- **Caching Impact**: Significantly reduces API calls for read operations
- **Recommended Pattern**: Cache frequently accessed configurations with appropriate TTL

### Cost Optimization

- **API Call Pricing**: $0.03 per 10,000 operations
- **Cache Strategy**: Longer TTL = fewer API calls = lower cost
- **Read vs Write**: Reads are typically 10x more frequent than writes

## API Reference

### Core Methods (IConfigureTagzApp)

```csharp
Task InitializeConfiguration(string providerName, string configurationString);
Task<T> GetConfigurationById<T>(string id) where T : new();
Task<string> GetConfigurationStringById(string id);
Task SetConfigurationById<T>(string id, T value);
```

### Extended Methods (AzureKeyVaultConfigureTagzApp)

```csharp
Task DeleteConfigurationById(string id);
Task<IEnumerable<string>> ListConfigurationIds();
```

### Configuration Classes

```csharp
public class AzureKeyVaultConfigurationOptions
{
    public string? KeyVaultUri { get; set; }
    public string? TenantId { get; set; }  
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string KeyPrefix { get; set; } = "TagzApp-";
    public int CacheTtlMinutes { get; set; } = 60;
    public bool UseManagedIdentity { get; set; } = true;
    
    public void Validate();
}
```

## Version History

### v1.0.0
- Initial release
- Managed Identity and Service Principal authentication
- Intelligent caching with configurable TTL
- Secret name sanitization
- Comprehensive logging and error handling
- Full IConfigureTagzApp interface implementation
- Additional management methods (List, Delete)

## License

This library is part of the TagzApp project and follows the same licensing terms as the main project.
