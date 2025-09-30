using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TagzApp.Common;
using Xunit;

namespace TagzApp.UnitTest.Common;

public class EncryptionHelperTests
{
	[Fact]
	public void EncryptDecrypt_WithValidKeys_ShouldRoundTrip()
	{
		// Arrange
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Encryption:Key"] = "zK6wT9+s2DxeYiycIySngGYSnlN96X1CjaXFZmLIFwM=", // 32 bytes
				["Encryption:IV"] = "KhTD/uhmPf75GEUxlz3trQ==" // 16 bytes
			})
			.Build();

		var encryptionHelper = new EncryptionHelper(config);
		const string originalText = "This is a test configuration value with sensitive data!";

		// Act
		var encrypted = encryptionHelper.Encrypt(originalText);
		var decrypted = encryptionHelper.Decrypt(encrypted);

		// Assert
		Assert.NotEqual(originalText, encrypted);
		Assert.Equal(originalText, decrypted);
	}

	[Fact]
	public void EncryptDecrypt_WithEmptyString_ShouldReturnEmptyString()
	{
		// Arrange
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Encryption:Key"] = "zK6wT9+s2DxeYiycIySngGYSnlN96X1CjaXFZmLIFwM=",
				["Encryption:IV"] = "KhTD/uhmPf75GEUxlz3trQ=="
			})
			.Build();

		var encryptionHelper = new EncryptionHelper(config);

		// Act & Assert
		Assert.Equal(string.Empty, encryptionHelper.Encrypt(string.Empty));
		Assert.Equal(string.Empty, encryptionHelper.Decrypt(string.Empty));
	}

	[Fact]
	public void Constructor_WithMissingKeys_ShouldEnterNoEncryptionMode()
	{
		// Arrange
		var config = new ConfigurationBuilder().Build();

		// Act - Should not throw exception
		var encryptionHelper = new EncryptionHelper(config);
		const string testValue = "test configuration value";
		var result = encryptionHelper.Encrypt(testValue);

		// Assert
		Assert.Equal(testValue, result); // Should return plain text when encryption is disabled
	}

	[Fact]
	public void NoEncryptionMode_ShouldPassThroughValues()
	{
		// Arrange
		var config = new ConfigurationBuilder().Build();
		var encryptionHelper = new EncryptionHelper(config);
		const string testValue = "plain text configuration";

		// Act
		var encrypted = encryptionHelper.Encrypt(testValue);
		var decrypted = encryptionHelper.Decrypt(testValue);

		// Assert
		Assert.Equal(testValue, encrypted);
		Assert.Equal(testValue, decrypted);
	}

	[Fact]
	public void Constructor_WithInvalidKeyLength_ShouldEnterNoEncryptionMode()
	{
		// Arrange
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Encryption:Key"] = "dGVzdA==", // "test" in base64 - only 4 bytes
				["Encryption:IV"] = "KhTD/uhmPf75GEUxlz3trQ=="
			})
			.Build();

		// Act - Should not throw exception, should enter no-encryption mode
		var encryptionHelper = new EncryptionHelper(config);
		const string testValue = "test value";
		var result = encryptionHelper.Encrypt(testValue);

		// Assert
		Assert.Equal(testValue, result); // Should return plain text when encryption is disabled
	}

	[Fact]
	public void Constructor_WithInvalidIVLength_ShouldEnterNoEncryptionMode()
	{
		// Arrange
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Encryption:Key"] = "zK6wT9+s2DxeYiycIySngGYSnlN96X1CjaXFZmLIFwM=",
				["Encryption:IV"] = "dGVzdA==" // "test" in base64 - only 4 bytes
			})
			.Build();

		// Act - Should not throw exception, should enter no-encryption mode
		var encryptionHelper = new EncryptionHelper(config);
		const string testValue = "test value";
		var result = encryptionHelper.Encrypt(testValue);

		// Assert
		Assert.Equal(testValue, result); // Should return plain text when encryption is disabled
	}

	[Fact]
	public void Constructor_WithInvalidBase64Key_ShouldEnterNoEncryptionMode()
	{
		// Arrange
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Encryption:Key"] = "invalid-base64-string!@#",
				["Encryption:IV"] = "KhTD/uhmPf75GEUxlz3trQ=="
			})
			.Build();

		// Act - Should not throw exception, should enter no-encryption mode
		var encryptionHelper = new EncryptionHelper(config);
		const string testValue = "test value";
		var result = encryptionHelper.Encrypt(testValue);

		// Assert
		Assert.Equal(testValue, result); // Should return plain text when encryption is disabled
	}

	[Fact]
	public void Decrypt_WithInvalidCipherText_ShouldThrowException()
	{
		// Arrange
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Encryption:Key"] = "zK6wT9+s2DxeYiycIySngGYSnlN96X1CjaXFZmLIFwM=",
				["Encryption:IV"] = "KhTD/uhmPf75GEUxlz3trQ=="
			})
			.Build();

		var encryptionHelper = new EncryptionHelper(config);

		// Act & Assert - Should throw when trying to decrypt invalid cipher text with encryption enabled
		Assert.Throws<InvalidOperationException>(() => encryptionHelper.Decrypt("invalid-cipher-text"));
	}
}
