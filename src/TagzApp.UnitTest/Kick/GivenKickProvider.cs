using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TagzApp.Providers.Kick;

namespace TagzApp.UnitTest.Kick;

public class GivenKickProvider
{
	[Fact]
	public void ShouldInitializeCorrectly()
	{
		// Arrange
		var mockLogger = new Mock<ILogger<KickProvider>>();
		var mockChatClient = new Mock<IChatClient>();
		var config = new KickConfiguration
		{
			ChannelName = "test-channel",
			ApiKey = "test-api-key",
			Enabled = true
		};
		var options = Options.Create(config);

		// Act
		var provider = new KickProvider(options, mockLogger.Object, mockChatClient.Object);

		// Assert
		Assert.Equal("KICK", provider.Id);
		Assert.Equal("Kick", provider.DisplayName);
		Assert.True(provider.Enabled);
		Assert.Contains("Kick is a streaming platform", provider.Description);
	}

	[Fact]
	public void ShouldReturnCorrectNewContentRetrievalFrequency()
	{
		// Arrange
		var mockLogger = new Mock<ILogger<KickProvider>>();
		var mockChatClient = new Mock<IChatClient>();
		var config = new KickConfiguration
		{
			ChannelName = "test-channel",
			Enabled = true
		};
		var options = Options.Create(config);

		// Act
		var provider = new KickProvider(options, mockLogger.Object, mockChatClient.Object);

		// Assert
		Assert.Equal(TimeSpan.FromSeconds(1), provider.NewContentRetrievalFrequency);
	}

	[Fact]
	public async Task ShouldReturnHealthyStatusWhenConnected()
	{
		// Arrange
		var mockLogger = new Mock<ILogger<KickProvider>>();
		var mockChatClient = new Mock<IChatClient>();
		mockChatClient.Setup(x => x.IsRunning).Returns(true);
		mockChatClient.Setup(x => x.IsConnected).Returns(true);

		var config = new KickConfiguration
		{
			ChannelName = "test-channel",
			Enabled = true
		};
		var options = Options.Create(config);

		var provider = new KickProvider(options, mockLogger.Object, mockChatClient.Object);

		// Act
		var (status, message) = await provider.GetHealth();

		// Assert
		Assert.Equal(SocialMediaStatus.Healthy, status);
		Assert.Equal("OK", message);
	}
}
