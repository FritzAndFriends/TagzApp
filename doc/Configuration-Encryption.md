# Configuration Encryption

TagzApp now supports encrypting sensitive configuration data stored in the database using AES-256 encryption. This provides an additional layer of security for configuration values such as API keys, connection strings, and other sensitive information.

## How It Works

When encryption is enabled, all configuration values stored in the `SystemConfiguration` table are automatically encrypted before being written to the database and decrypted when retrieved. The encryption is transparent to application code - your providers and services continue to work exactly as before.

**When encryption is not configured or misconfigured, TagzApp automatically enters a "no encryption state" where configuration data is stored in plain text, with appropriate warnings logged.**

## Configuration

### 1. Generate Encryption Keys

You need two keys:
- **Encryption Key**: 32 bytes (256-bit) for AES encryption  
- **Initialization Vector (IV)**: 16 bytes (128-bit) for AES encryption

#### Using PowerShell (Windows/Linux/macOS)

```powershell
# Generate encryption key (32 bytes, base64 encoded)
[System.Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# Generate IV (16 bytes, base64 encoded)
[System.Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(16))
```

#### Using .NET CLI

```bash
# Create a simple console app to generate keys
dotnet new console -n KeyGenerator
cd KeyGenerator
# Add this code to Program.cs:
# Console.WriteLine("Key: " + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
# Console.WriteLine("IV: " + Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)));
dotnet run
```

### 2. Configure in appsettings.json

Add the encryption configuration to your `appsettings.json`:

```json
{
  "Encryption": {
    "Key": "your-32-byte-base64-key-here",
    "IV": "your-16-byte-base64-iv-here"
  }
}
```

**Example with sample keys (DO NOT use these in production):**

```json
{
  "Encryption": {
    "Key": "zK6wT9+s2DxeYiycIySngGYSnlN96X1CjaXFZmLIFwM=",
    "IV": "KhTD/uhmPf75GEUxlz3trQ=="
  }
}
```

### 3. Environment Variables (Recommended for Production)

For production environments, use environment variables instead of storing keys in configuration files:

```bash
# Set environment variables
export Encryption__Key="your-32-byte-base64-key-here"
export Encryption__IV="your-16-byte-base64-iv-here"
```

Or in Docker:

```yaml
environment:
  - Encryption__Key=your-32-byte-base64-key-here
  - Encryption__IV=your-16-byte-base64-iv-here
```

## Encryption States

TagzApp operates in one of two encryption states:

### 🔐 Encryption Enabled
- Valid encryption keys are configured
- All configuration data is encrypted before storage
- Data is automatically decrypted when retrieved
- Application logs: "Configuration encryption is ENABLED using AES-256-CBC encryption"

### ⚠️ No Encryption State (Fallback)
- No encryption keys configured, or keys are invalid
- Configuration data is stored in **plain text**
- Warning messages are logged about the lack of encryption
- Application continues to function normally
- Application logs: "Configuration encryption is DISABLED..."

## Security Best Practices

### Production Key Management

**⚠️ NEVER store encryption keys in source control or configuration files in production!**

Use one of these secure alternatives:

#### Azure Key Vault

```csharp
// Configure in Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

#### AWS Secrets Manager

```csharp
// Add AWS configuration provider
builder.Configuration.AddSecretsManager();
```

#### Environment Variables

- Set via deployment scripts
- Use container orchestration secrets
- Configure through CI/CD pipelines

#### Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: tagzapp-encryption
type: Opaque
stringData:
  encryption-key: "your-base64-key"
  encryption-iv: "your-base64-iv"
```

### Key Rotation

To rotate encryption keys:

1. **Generate new keys** using the same method as initial setup
2. **Deploy with both old and new keys** temporarily
3. **Migrate data** by reading with old keys and writing with new keys
4. **Remove old keys** after migration is complete

### Key Requirements

- **Key**: Must be exactly 32 bytes (256 bits) when base64 decoded
- **IV**: Must be exactly 16 bytes (128 bits) when base64 decoded
- **Both must be cryptographically random** - never use predictable values
- **IV should be unique per deployment** but can be reused across configuration values

## Operational Behavior

### Graceful Degradation

The encryption system is designed to never break your application:

- **Missing keys**: Enters no-encryption state with warnings
- **Invalid keys**: Enters no-encryption state with error logs
- **Key format errors**: Enters no-encryption state with error logs
- **Decryption failures**: Throws detailed error messages for troubleshooting

### Logging

The application logs encryption status at startup and during operation:

**Encryption Enabled:**
```
info: TagzApp.Common.EncryptionHelper[0]
      Configuration encryption is ENABLED using AES-256-CBC encryption.
```

**No Encryption (Warning):**
```
warn: TagzApp.Common.EncryptionHelper[0]
      Configuration encryption is DISABLED. Encryption:Key and Encryption:IV are not configured. Configuration data will be stored in PLAIN TEXT. This is not recommended for production environments.
```

**Invalid Configuration (Error):**
```
fail: TagzApp.Common.EncryptionHelper[0]
      Configuration encryption is DISABLED. Encryption key must be 32 bytes (256 bits) when base64 decoded, but was 4 bytes. Configuration data will be stored in PLAIN TEXT.
```

## Backward Compatibility

The encryption feature is fully backward compatible:

- **No encryption configured**: Data is stored in plain text (existing behavior)
- **Encryption configured**: New data is encrypted, existing plain text data continues to work
- **Mixed environments**: You can gradually roll out encryption without breaking existing deployments
- **Configuration changes**: Can switch between encrypted and non-encrypted modes seamlessly

## Data Migration

If you want to encrypt existing plain text configuration data:

1. **Configure encryption keys** as described above
2. **Restart the application** - new configuration writes will be encrypted
3. **Re-save existing configurations** through the admin interface to encrypt them
4. **Or run a migration script** to bulk encrypt existing data

## Troubleshooting

### Common Errors

**"Configuration encryption is DISABLED. Encryption:Key and Encryption:IV are not configured"**
- The encryption section is missing from configuration
- Environment variables are not set correctly
- **Resolution**: Application continues in no-encryption state with warnings

**"Encryption keys must be valid Base64 strings"**
- The key or IV contains invalid base64 characters
- Copy-paste error or encoding issue
- **Resolution**: Application continues in no-encryption state with error logs

**"Encryption key must be 32 bytes (256 bits)"**
- The decoded key is not exactly 32 bytes
- Re-generate the key using the provided methods
- **Resolution**: Application continues in no-encryption state with error logs

**"Failed to decrypt configuration value"**
- Data was encrypted with different keys
- Database corruption
- Key rotation without proper migration
- **Resolution**: Application throws detailed error for investigation

### Verifying Configuration

Check if encryption is working by:

1. **Check application logs** for encryption status messages at startup
2. **Set a configuration value** through the admin interface
3. **Check the database directly** - the `Value` column should contain base64-encoded encrypted data (if encryption is enabled)
4. **Verify the application can read** the configuration correctly

### Development vs Production

**Development Environment:**
- Can run without encryption (warnings logged)
- Keys can be in appsettings.json for convenience
- Plain text storage is acceptable for development

**Production Environment:**
- Should always use encryption
- Keys must be in secure storage (environment variables, key vaults)
- Monitor logs for encryption status warnings

## Implementation Details

The encryption implementation uses:

- **Algorithm**: AES-256-CBC
- **Key derivation**: Direct key usage (no PBKDF2)
- **Padding**: PKCS7
- **IV**: Fixed IV per deployment (configurable)
- **Fallback**: Plain text storage when encryption unavailable

All configuration values are:
1. **Serialized** to JSON
2. **Encrypted** using AES-256-CBC (if encryption enabled)
3. **Base64 encoded** for database storage
4. **Base64 decoded** on retrieval
5. **Decrypted** using AES-256-CBC (if encryption enabled)
6. **Deserialized** from JSON

The process is transparent to application code and maintains full compatibility with existing provider configurations.
