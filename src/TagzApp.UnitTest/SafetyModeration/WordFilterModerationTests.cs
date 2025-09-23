using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TagzApp.Storage.Postgres.SafetyModeration;
using Xunit;

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