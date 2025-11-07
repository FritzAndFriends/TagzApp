using TagzApp.Configuration.AzureKeyVault;
using Xunit;

namespace TagzApp.UnitTest.Configuration;

public class AzureKeyVaultConfigurationOptionsTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = true
        };

        // Act & Assert
        options.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_WithMissingKeyVaultUri_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            UseManagedIdentity = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal("KeyVaultUri is required (Parameter 'KeyVaultUri')", exception.Message);
    }

    [Fact]
    public void Validate_WithInvalidKeyVaultUri_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "not-a-valid-uri",
            UseManagedIdentity = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal("KeyVaultUri must be a valid URI (Parameter 'KeyVaultUri')", exception.Message);
    }

    [Fact]
    public void Validate_WithServicePrincipalMissingClientId_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = false,
            ClientSecret = "secret"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal("ClientId is required when not using managed identity (Parameter 'ClientId')", exception.Message);
    }

    [Fact]
    public void Validate_WithServicePrincipalMissingClientSecret_ThrowsArgumentException()
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = false,
            ClientId = "client-id"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal("ClientSecret is required when not using managed identity (Parameter 'ClientSecret')", exception.Message);
    }

    [Fact]
    public void Validate_WithServicePrincipalValidCredentials_ShouldNotThrow()
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = false,
            TenantId = "tenant-id",
            ClientId = "client-id",
            ClientSecret = "client-secret"
        };

        // Act & Assert
        options.Validate(); // Should not throw
    }

    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var options = new AzureKeyVaultConfigurationOptions();

        // Assert
        Assert.Equal("TagzApp-", options.KeyPrefix);
        Assert.Equal(60, options.CacheTtlMinutes);
        Assert.True(options.UseManagedIdentity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyKeyVaultUri_ThrowsArgumentException(string? keyVaultUri)
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = keyVaultUri,
            UseManagedIdentity = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal("KeyVaultUri is required (Parameter 'KeyVaultUri')", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithServicePrincipalEmptyClientId_ThrowsArgumentException(string? clientId)
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = false,
            ClientId = clientId,
            ClientSecret = "secret"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal("ClientId is required when not using managed identity (Parameter 'ClientId')", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithServicePrincipalEmptyClientSecret_ThrowsArgumentException(string? clientSecret)
    {
        // Arrange
        var options = new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = false,
            ClientId = "client-id",
            ClientSecret = clientSecret
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Equal("ClientSecret is required when not using managed identity (Parameter 'ClientSecret')", exception.Message);
    }
}
