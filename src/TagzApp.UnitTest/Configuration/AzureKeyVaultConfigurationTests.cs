using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TagzApp.Configuration.AzureKeyVault;
using Xunit;

namespace TagzApp.UnitTest.Configuration;

public class AzureKeyVaultConfigurationTests
{
    private readonly ILogger<AzureKeyVaultConfigureTagzApp> _logger;

    public AzureKeyVaultConfigurationTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        _logger = loggerFactory.CreateLogger<AzureKeyVaultConfigureTagzApp>();
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AzureKeyVaultConfigureTagzApp(null!, _logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/"
        });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AzureKeyVaultConfigureTagzApp(options, null!));
    }

    [Fact]
    public void Constructor_WithMissingKeyVaultUri_ThrowsArgumentException()
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new AzureKeyVaultConfigureTagzApp(options, _logger));
    }

    [Fact]
    public void Constructor_WithInvalidKeyVaultUri_ThrowsArgumentException()
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "not-a-valid-uri"
        });

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new AzureKeyVaultConfigureTagzApp(options, _logger));
    }

    [Fact]
    public void Constructor_WithMissingClientCredentialsWhenNotUsingManagedIdentity_ThrowsArgumentException()
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = false
        });

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new AzureKeyVaultConfigureTagzApp(options, _logger));
    }

    [Fact]
    public async Task InitializeConfiguration_ShouldCompleteSuccessfully()
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = true
        });

        using var provider = new AzureKeyVaultConfigureTagzApp(options, _logger);

        // Act & Assert
        await provider.InitializeConfiguration("test-provider", "test-config");
        // Should not throw
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetConfigurationById_WithInvalidId_ThrowsArgumentException(string invalidId)
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = true
        });

        using var provider = new AzureKeyVaultConfigureTagzApp(options, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.GetConfigurationById<TestConfiguration>(invalidId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SetConfigurationById_WithInvalidId_ThrowsArgumentException(string invalidId)
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = true
        });

        using var provider = new AzureKeyVaultConfigureTagzApp(options, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.SetConfigurationById(invalidId, new TestConfiguration()));
    }

    [Fact]
    public async Task SetConfigurationById_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new AzureKeyVaultConfigurationOptions
        {
            KeyVaultUri = "https://test.vault.azure.net/",
            UseManagedIdentity = true
        });

        using var provider = new AzureKeyVaultConfigureTagzApp(options, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SetConfigurationById<TestConfiguration>("test", null!));
    }

    private class TestConfiguration
    {
        public string Value { get; set; } = string.Empty;
        public int Number { get; set; }
    }
}
