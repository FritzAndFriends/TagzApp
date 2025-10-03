namespace TagzApp.UnitTest.SafetyModeration;

public class WordFilterModerationTests
{
	[Fact]
	public void WordFilterConfiguration_ShouldHaveCorrectConfigurationKey()
	{
		// Assert
		Assert.Equal("wordfilter", WordFilterConfiguration.ConfigurationKey);
	}

	[Fact]
	public void WordFilterConfiguration_ShouldInitializeWithDefaults()
	{
		// Act
		var config = new WordFilterConfiguration();

		// Assert
		Assert.False(config.Enabled);
		Assert.Empty(config.BlockedWords);
	}

	[Fact]
	public void WordFilterConfiguration_ShouldAllowSettingProperties()
	{
		// Arrange
		var blockedWords = new[] { "test", "example" };

		// Act
		var config = new WordFilterConfiguration
		{
			Enabled = true,
			BlockedWords = blockedWords
		};

		// Assert
		Assert.True(config.Enabled);
		Assert.Equal(blockedWords, config.BlockedWords);
	}
}
