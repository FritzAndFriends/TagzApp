namespace TagzApp.UnitTest.InMemoryMessaging.GivenOneSubscriber;
public class WhenPublishingMessages
{

	protected InMemoryContentMessaging _Sut = new();

	private readonly Hashtag _Tag = new() { Text = "Test" };

	private readonly Content _Content = new()
	{
		Author = new Creator
		{
			DisplayName = "Testy McTestface",
			ProfileImageUri = new Uri("http://myta.g"),
			ProfileUri = new Uri("http://myta.gg"),
		},
		Provider = "TEST",
		ProviderId = "test-id",
		SourceUri = new Uri("http://myta.gg/1"),
		Text = "This is a test",
		Timestamp = DateTimeOffset.Now,
		Type = ContentType.Message
	};

	[Fact]
	public async Task ShouldPublishMessage()
	{

		// Arrange
		Content published = null!;
		_Sut.SubscribeToContent(_Tag, (content) => published = content);

		// Act
		await _Sut.PublishContentAsync(_Tag, _Content);
		await Task.Delay(200);

		// Assert
		Assert.NotNull(published);
		Assert.Equal("This is a test", published.Text);

	}

}
