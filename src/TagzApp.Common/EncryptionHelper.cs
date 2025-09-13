using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace TagzApp.Common;

public class EncryptionHelper
{
    private readonly byte[]? _Key;
    private readonly byte[]? _IV;
    private readonly ILogger<EncryptionHelper>? _Logger;
    private readonly bool _EncryptionEnabled;

    public EncryptionHelper(IConfiguration config, ILogger<EncryptionHelper>? logger = null)
    {
        _Logger = logger;
        
        var keyString = config["Encryption:Key"];
        var ivString = config["Encryption:IV"];

        if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
        {
            _EncryptionEnabled = false;
            _Logger?.LogWarning("Configuration encryption is DISABLED. Encryption:Key and Encryption:IV are not configured. Configuration data will be stored in PLAIN TEXT. This is not recommended for production environments.");
            return;
        }

        try
        {
            _Key = Convert.FromBase64String(keyString);
            _IV = Convert.FromBase64String(ivString);
        }
        catch (FormatException ex)
        {
            _EncryptionEnabled = false;
            _Logger?.LogError(ex, "Configuration encryption is DISABLED due to invalid encryption keys. Encryption keys must be valid Base64 strings. Configuration data will be stored in PLAIN TEXT.");
            return;
        }

        if (_Key.Length != 32)
        {
            _EncryptionEnabled = false;
            _Logger?.LogError("Configuration encryption is DISABLED. Encryption key must be 32 bytes (256 bits) when base64 decoded, but was {KeyLength} bytes. Configuration data will be stored in PLAIN TEXT.", _Key.Length);
            return;
        }

        if (_IV.Length != 16)
        {
            _EncryptionEnabled = false;
            _Logger?.LogError("Configuration encryption is DISABLED. Encryption IV must be 16 bytes (128 bits) when base64 decoded, but was {IVLength} bytes. Configuration data will be stored in PLAIN TEXT.", _IV.Length);
            return;
        }

        _EncryptionEnabled = true;
        _Logger?.LogInformation("Configuration encryption is ENABLED using AES-256-CBC encryption.");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        if (!_EncryptionEnabled)
        {
            _Logger?.LogDebug("Encryption is disabled, returning plain text value");
            return plainText;
        }

        using var aes = Aes.Create();
        aes.Key = _Key!;
        aes.IV = _IV!;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        if (!_EncryptionEnabled)
        {
            _Logger?.LogDebug("Encryption is disabled, returning cipher text as plain text");
            return cipherText;
        }

        try
        {
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _Key!;
            aes.IV = _IV!;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decrypt configuration value. This may indicate data corruption or incorrect encryption keys.", ex);
        }
    }
}
