# Azure Key Vault Configuration Provider - Example Usage

This example demonstrates how to use the Azure Key Vault configuration provider in TagzApp.

## Example Application Setup

```csharp
using TagzApp.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateDefaultBuilder(args);

// Configure the host
builder.ConfigureServices((context, services) =>
{
    // Option 1: Use configuration from appsettings.json
    services.AddAzureKeyVaultConfiguration(context.Configuration);
    
    // Option 2: Configure with Managed Identity
    /*
    services.AddAzureKeyVaultConfigurationWithManagedIdentity(
        "https://your-keyvault.vault.azure.net/",
        keyPrefix: "TagzApp-",
        cacheTtlMinutes: 60);
    */
    
    // Option 3: Configure with Service Principal
    /*
    services.AddAzureKeyVaultConfigurationWithServicePrincipal(
        keyVaultUri: "https://your-keyvault.vault.azure.net/",
        tenantId: "your-tenant-id",
        clientId: "your-client-id", 
        clientSecret: "your-client-secret");
    */
});

var host = builder.Build();

// Example usage
var configProvider = host.Services.GetRequiredService<IConfigureTagzApp>();

// Store configuration
var twitterConfig = new TwitterConfiguration
{
    ApiKey = "your-api-key",
    ApiSecret = "your-api-secret",
    BearerToken = "your-bearer-token"
};

await configProvider.SetConfigurationById("twitter-config", twitterConfig);

// Retrieve configuration  
var retrievedConfig = await configProvider.GetConfigurationById<TwitterConfiguration>("twitter-config");

Console.WriteLine($"Retrieved API Key: {retrievedConfig.ApiKey}");

await host.RunAsync();
```

## Configuration File (appsettings.json)

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

## Configuration Classes

```csharp
public class TwitterConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string BearerToken { get; set; } = string.Empty;
}

public class MastodonConfiguration
{
    public string Instance { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
```

## Environment Variables (Production)

For production deployments, use environment variables instead of configuration files:

```bash
# Azure App Service / Container Apps
AzureKeyVault__KeyVaultUri=https://your-keyvault.vault.azure.net/
AzureKeyVault__UseManagedIdentity=true
AzureKeyVault__KeyPrefix=TagzApp-
AzureKeyVault__CacheTtlMinutes=60

# Service Principal (if not using Managed Identity)
AzureKeyVault__UseManagedIdentity=false
AzureKeyVault__TenantId=your-tenant-id
AzureKeyVault__ClientId=your-client-id
AzureKeyVault__ClientSecret=your-client-secret
```

## Migration from Database Configuration

If you're migrating from the database configuration provider:

```csharp
public async Task MigrateToAzureKeyVault()
{
    var dbProvider = serviceProvider.GetService<DbConfigureTagzApp>();
    var azureProvider = serviceProvider.GetService<AzureKeyVaultConfigureTagzApp>();
    
    // List of configuration IDs to migrate (you'll need to implement this)
    var configIds = new[] { "twitter-config", "mastodon-config", "youtube-config" };
    
    foreach (var configId in configIds)
    {
        try
        {
            // Get from database
            var value = await dbProvider.GetConfigurationStringById(configId);
            
            if (!string.IsNullOrEmpty(value))
            {
                // The value is already JSON serialized from the database
                var deserializedValue = JsonSerializer.Deserialize<object>(value);
                
                // Store in Azure Key Vault
                await azureProvider.SetConfigurationById(configId, deserializedValue);
                
                Console.WriteLine($"Migrated configuration: {configId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to migrate {configId}: {ex.Message}");
        }
    }
}
```

## Azure Key Vault Setup

### 1. Create Key Vault

```bash
# Create resource group
az group create --name TagzApp-RG --location eastus

# Create Key Vault  
az keyvault create \
  --name TagzApp-KeyVault \
  --resource-group TagzApp-RG \
  --location eastus \
  --sku standard
```

### 2. Grant Permissions

For Managed Identity:
```bash
# Get the principal ID of your App Service/Container App
PRINCIPAL_ID=$(az webapp identity show --name your-app --resource-group TagzApp-RG --query principalId -o tsv)

# Grant Key Vault permissions
az keyvault set-policy \
  --name TagzApp-KeyVault \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get set list delete
```

For Service Principal:
```bash
# Create service principal
az ad sp create-for-rbac --name TagzApp-ServicePrincipal

# Grant Key Vault permissions  
az keyvault set-policy \
  --name TagzApp-KeyVault \
  --spn <service-principal-app-id> \
  --secret-permissions get set list delete
```

### 3. Test Access

```bash
# Test secret creation
az keyvault secret set \
  --vault-name TagzApp-KeyVault \
  --name TagzApp-test-config \
  --value '{"test": "value"}'

# Test secret retrieval
az keyvault secret show \
  --vault-name TagzApp-KeyVault \
  --name TagzApp-test-config
```

This example provides a complete working setup for using Azure Key Vault as your TagzApp configuration store!
