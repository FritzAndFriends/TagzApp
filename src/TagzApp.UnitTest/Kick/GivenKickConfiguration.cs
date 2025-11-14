using TagzApp.Providers.Kick;

namespace TagzApp.UnitTest.Kick;

public class GivenKickConfiguration
{
	[Fact]
	public void ShouldInitializeWithCorrectDefaults()
	{
		// Act
		var config = new KickConfiguration();

		// Assert
		Assert.Equal("Kick", config.Name);
		Assert.Equal("Read all messages from a specified Kick channel", config.Description);
		Assert.Equal("provider-kick", KickConfiguration.GetConfigurationKey());
		Assert.False(config.Enabled);
		Assert.Empty(config.ChannelName);
		Assert.Empty(config.ApiKey);
	}

	[Fact]
	public void ShouldHandleConfigurationByKey()
	{
		// Arrange
		var config = new KickConfiguration();

		// Act
		config.SetConfigurationByKey("ChannelName", "test-channel");
		config.SetConfigurationByKey("ApiKey", "test-api-key");
		config.SetConfigurationByKey("Enabled", "true");

		// Assert
		Assert.Equal("test-channel", config.GetConfigurationByKey("ChannelName"));
		Assert.Equal("test-api-key", config.GetConfigurationByKey("ApiKey"));
		Assert.Equal("True", config.GetConfigurationByKey("Enabled"));
		Assert.True(config.Enabled);
	}

	[Fact]
	public void ShouldReturnCorrectKeys()
	{
		// Arrange
		var config = new KickConfiguration();

		// Act
		var keys = config.Keys;

		// Assert
		Assert.Contains("ChannelName", keys);
		Assert.Contains("ApiKey", keys);
		Assert.Equal(2, keys.Length);
	}

	[Fact]
	public void ShouldUpdateFromAnotherConfiguration()
	{
		// Arrange
		var config1 = new KickConfiguration
		{
			ChannelName = "channel1",
			ApiKey = "key1",
			Enabled = true
		};

		var config2 = new KickConfiguration();

		// Act
		config2.UpdateFrom(config1);

		// Assert
		Assert.Equal("channel1", config2.ChannelName);
		Assert.Equal("key1", config2.ApiKey);
		Assert.True(config2.Enabled);
	}
}
