namespace TagzApp.UnitTest.InMemoryMessaging.GivenNoSubscribers;

public class WhenPublishingMessages : BaseFixture
{

	private Hashtag _Tag = new() { Text = "test" };

	// Arrange - PUBLISH SOME MESSAGES
	private async Task Arrange()
	{

		await _Sut.PublishContentAsync(_Tag, new Content
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
		});
	}

	[Fact]
	public async Task ShouldNotHoldMessages()
	{

		await Arrange();

		await Task.Delay(200);

		Assert.Empty(_Sut.Queue[_Tag.Text]);

	}

}
