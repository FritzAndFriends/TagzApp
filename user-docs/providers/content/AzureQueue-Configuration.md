# Azure Queue Provider Configuration Guide

This guide will help you configure the Azure Queue content provider for TagzApp to read custom messages from Azure Storage Queue.

## Overview

The Azure Queue provider allows TagzApp to read messages from a custom Azure Storage Queue. This is useful for integrating custom data sources, third-party applications, or automated systems that post content to TagzApp via Azure Queue Storage.

## Prerequisites

- An Azure account
- An Azure Storage Account
- Access to Azure Portal or Azure CLI
- Basic understanding of Azure Queue Storage

## Use Cases

The Azure Queue provider is ideal for:
- **Custom integrations**: Push content from internal systems
- **Third-party services**: Integrate services without direct API support
- **Automated content**: Post scheduled or triggered content
- **Legacy systems**: Connect systems that can write to Azure Queue
- **Bulk operations**: Queue multiple messages for processing

## Step 1: Create an Azure Storage Account

### Using Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Sign in with your Azure account
3. Click **"Create a resource"**
4. Search for **"Storage account"**
5. Click **"Create"**
6. Configure the storage account:
   - **Subscription**: Select your subscription
   - **Resource group**: Create new or select existing
   - **Storage account name**: Enter a unique name (lowercase, numbers only)
   - **Region**: Select your preferred region
   - **Performance**: Standard (unless you need Premium)
   - **Redundancy**: Choose based on your needs (LRS is cheapest)
7. Click **"Review + create"** and then **"Create"**
8. Wait for deployment to complete

### Using Azure CLI

```bash
# Create resource group (if needed)
az group create --name TagzAppResources --location eastus

# Create storage account
az storage account create \
  --name tagzappstorage \
  --resource-group TagzAppResources \
  --location eastus \
  --sku Standard_LRS
```

## Step 2: Create a Queue

### Using Azure Portal

1. Navigate to your Storage Account
2. In the left menu, click **"Queues"** under "Data storage"
3. Click **"+ Queue"**
4. Enter a queue name (e.g., `tagzapp-messages`)
5. Click **"OK"**

### Using Azure CLI

```bash
# Get connection string
CONNECTION_STRING=$(az storage account show-connection-string \
  --name tagzappstorage \
  --resource-group TagzAppResources \
  --query connectionString -o tsv)

# Create queue
az storage queue create \
  --name tagzapp-messages \
  --connection-string "$CONNECTION_STRING"
```

## Step 3: Get Your Connection String

### Using Azure Portal

1. Navigate to your Storage Account
2. In the left menu, click **"Access keys"** under "Security + networking"
3. You'll see two keys (key1 and key2)
4. Click **"Show keys"** to reveal connection strings
5. Copy the **"Connection string"** for either key
   - Example: `DefaultEndpointsProtocol=https;AccountName=tagzappstorage;AccountKey=xxx...;EndpointSuffix=core.windows.net`

### Using Azure CLI

```bash
az storage account show-connection-string \
  --name tagzappstorage \
  --resource-group TagzAppResources \
  --query connectionString -o tsv
```

**⚠️ Security Warning:** Connection strings provide full access to your storage account. Keep them secure and never commit them to source control.

## Step 4: Configure TagzApp

### Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > Providers**
3. Find the **"AzureQueue"** provider
4. Click **"Configure"**
5. Enter your configuration:
   - **Queue Connection String**: Paste your Azure Storage connection string
   - **Enabled**: Toggle to enable the provider
6. Click **"Save Configuration"**

### Using Configuration Files

**appsettings.json:**
```json
{
  "providers": {
    "azurequeue": {
      "Enabled": true,
      "QueueConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
    }
  }
}
```

**Environment Variables:**
```bash
providers__azurequeue__Enabled=true
providers__azurequeue__QueueConnectionString="DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
```

**Azure Key Vault:**
Store with the key:
```
TagzApp-providers-azurequeue
```

## Step 5: Send Messages to the Queue

### Message Format

Messages sent to the Azure Queue should follow the TagzApp content format. The exact schema depends on your TagzApp version, but typically includes:

```json
{
  "provider": "AzureQueue",
  "author": {
    "displayName": "User Name",
    "userName": "username",
    "profileImage": "https://example.com/avatar.jpg"
  },
  "content": "Message content with #hashtag",
  "timestamp": "2024-01-15T12:00:00Z",
  "sourceUri": "https://source.example.com/message/123"
}
```

### Using Azure Portal

1. Navigate to your Storage Account
2. Click **"Queues"**
3. Select your queue
4. Click **"+ Add message"**
5. Enter your JSON message
6. Click **"OK"**

### Using Azure CLI

```bash
az storage message put \
  --queue-name tagzapp-messages \
  --content '{"provider":"AzureQueue","author":{"displayName":"Test User"},"content":"Test message #test"}' \
  --connection-string "$CONNECTION_STRING"
```

### Using C# Code

```csharp
using Azure.Storage.Queues;
using System.Text.Json;

var connectionString = "your-connection-string";
var queueName = "tagzapp-messages";

var queueClient = new QueueClient(connectionString, queueName);
await queueClient.CreateIfNotExistsAsync();

var message = new {
    provider = "AzureQueue",
    author = new {
        displayName = "User Name",
        userName = "username"
    },
    content = "Message content #hashtag",
    timestamp = DateTime.UtcNow.ToString("o"),
    sourceUri = "https://example.com/message/123"
};

var messageJson = JsonSerializer.Serialize(message);
await queueClient.SendMessageAsync(messageJson);
```

### Using PowerShell

```powershell
$connectionString = "your-connection-string"
$queueName = "tagzapp-messages"

$context = New-AzStorageContext -ConnectionString $connectionString
$queue = Get-AzStorageQueue -Name $queueName -Context $context

$message = @{
    provider = "AzureQueue"
    author = @{
        displayName = "User Name"
        userName = "username"
    }
    content = "Message content #hashtag"
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

$queue.CloudQueue.AddMessage($message)
```

## Step 6: Test Your Configuration

1. Send a test message to your Azure Queue (using any method above)
2. Wait a few seconds for TagzApp to poll the queue
3. The message should appear in TagzApp
4. Check TagzApp logs if messages don't appear

## Troubleshooting

### Common Issues

#### "Invalid Connection String" Error
- **Cause**: Connection string is malformed or incorrect
- **Solution**: 
  - Verify you copied the entire connection string
  - Check for extra spaces or line breaks
  - Regenerate connection string in Azure Portal

#### "Queue Not Found" Error
- **Cause**: Queue doesn't exist or name mismatch
- **Solution**: 
  - Verify the queue exists in your storage account
  - Check queue name spelling
  - Create the queue if it doesn't exist

#### "Authentication Failed" Error
- **Cause**: Invalid account key or SAS token
- **Solution**: 
  - Regenerate access keys in Azure Portal
  - Update TagzApp configuration
  - Check if connection string has expired

#### No Messages Appearing
- **Cause**: Various reasons
- **Solution**:
  - Verify the provider is enabled in TagzApp
  - Check if messages are in the queue (Azure Portal)
  - Verify message format is correct
  - Check TagzApp logs for parsing errors
  - Ensure queue connection string is correct

#### Messages Disappearing Without Processing
- **Cause**: Invalid message format
- **Solution**: 
  - Check message schema matches expected format
  - Review TagzApp logs for parsing errors
  - Validate JSON structure

## Queue Behavior

### Message Processing

- TagzApp polls the queue periodically
- Messages are retrieved and processed
- Successfully processed messages are deleted from the queue
- Failed messages may be retried (depending on configuration)

### Message Visibility

- Messages have a visibility timeout (default 30 seconds)
- If TagzApp crashes during processing, message becomes visible again
- Prevents message loss during failures

### Message Retention

- Azure Queue messages are retained for up to 7 days by default
- Unprocessed messages older than retention period are deleted
- Configure retention period in Azure Portal if needed

## Security Best Practices

1. **Use Azure Key Vault**: Store connection strings in Azure Key Vault
2. **Rotate keys regularly**: Regenerate storage account keys periodically
3. **Use Managed Identity**: If TagzApp runs in Azure, use managed identity instead of connection strings
4. **Limit access**: Use SAS tokens with minimal permissions instead of full connection strings
5. **Monitor access**: Review Azure Storage logs for unauthorized access
6. **Encrypt at rest**: Enable storage account encryption (enabled by default)

## Using Shared Access Signatures (SAS)

For better security, use SAS tokens instead of full connection strings:

### Generate SAS Token

```bash
# Generate SAS token for queue
az storage queue generate-sas \
  --name tagzapp-messages \
  --account-name tagzappstorage \
  --permissions raup \
  --expiry 2025-12-31 \
  --https-only \
  --connection-string "$CONNECTION_STRING"
```

### Use SAS Token in Connection String

```
BlobEndpoint=https://tagzappstorage.blob.core.windows.net/;QueueEndpoint=https://tagzappstorage.queue.core.windows.net/;SharedAccessSignature=sv=2021-06-08&ss=q&srt=sco&sp=raup&se=2025-12-31T...
```

## Cost Considerations

### Azure Storage Pricing

- **Queue operations**: ~$0.0036 per 10,000 operations
- **Data storage**: ~$0.02 per GB per month
- **Data transfer**: Free for data in, varies for data out

### Typical Costs for TagzApp

- Small events: < $1/month
- Medium events: $1-5/month
- Large events: $5-20/month

### Cost Optimization

1. **Delete processed messages**: TagzApp does this automatically
2. **Monitor queue length**: Prevent unbounded growth
3. **Use appropriate redundancy**: LRS is cheapest
4. **Set message retention**: Don't store messages longer than needed

## Advanced Configuration

### Using Managed Identity (Azure)

If TagzApp runs in Azure (App Service, Container, VM):

1. Enable managed identity on your Azure resource
2. Grant identity "Storage Queue Data Contributor" role
3. Use managed identity authentication instead of connection string

**Code example:**
```csharp
var queueServiceClient = new QueueServiceClient(
    new Uri("https://tagzappstorage.queue.core.windows.net"),
    new DefaultAzureCredential()
);
```

### Multiple Queues

To integrate multiple sources:
1. Create separate queues for each source
2. Configure multiple AzureQueue provider instances (requires code changes)
3. Or process all messages through a single queue with source identification

## Integration Examples

### Webhook to Queue

Create an Azure Function to receive webhooks and post to queue:

```csharp
[FunctionName("WebhookToQueue")]
public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
    [Queue("tagzapp-messages")] IAsyncCollector<string> queueMessages)
{
    var content = await new StreamReader(req.Body).ReadToEndAsync();
    await queueMessages.AddAsync(content);
    return new OkResult();
}
```

### Scheduled Messages

Use Azure Logic Apps or Functions to post scheduled content to the queue.

## Monitoring

### Azure Portal

1. Navigate to your Storage Account
2. Click **"Monitoring"** > **"Metrics"**
3. View queue operations, message count, etc.

### Application Insights

Integrate TagzApp with Application Insights to track:
- Message processing times
- Error rates
- Queue poll frequency

## Additional Resources

- [Azure Queue Storage Documentation](https://docs.microsoft.com/azure/storage/queues/)
- [Azure Storage Explorer](https://azure.microsoft.com/features/storage-explorer/)
- [Azure CLI Reference](https://docs.microsoft.com/cli/azure/storage/queue)
- [.NET Azure Queue Client Library](https://docs.microsoft.com/dotnet/api/overview/azure/storage.queues-readme)
- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review Azure Storage Queue logs and metrics
3. Consult Azure Storage Queue documentation
4. Open a new issue on GitHub with details about your problem
