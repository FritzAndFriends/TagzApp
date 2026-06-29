using TagzApp.ViewModels.Data;

namespace TagzApp.UnitTest.ViewModels;

public class ModerationContentFilterTests
{
	[Fact]
	public void ApplyModerationUpdate_RemovesApprovedContentFromPendingView()
	{
		var existing = CreateContent(ModerationState.Pending, providerId: "one");
		var content = new Dictionary<string, ModerationContentModel>
		{
			[ModerationContentFilter.GetContentKey(existing)] = existing
		};
		var approved = CreateContent(
			ModerationState.Approved,
			providerId: "one",
			reason: "Reviewed",
			moderator: "MODERATOR");

		ModerationContentFilter.ApplyModerationUpdate(content, approved, filterState: (int)ModerationState.Pending);

		Assert.Empty(content);
	}

	[Fact]
	public void ApplyModerationUpdate_ReplacesExistingContentWhenFilterMatches()
	{
		var existing = CreateContent(ModerationState.Pending, providerId: "one");
		var content = new Dictionary<string, ModerationContentModel>
		{
			[ModerationContentFilter.GetContentKey(existing)] = existing
		};
		var approved = CreateContent(
			ModerationState.Approved,
			providerId: "one",
			reason: "Reviewed",
			moderator: "MODERATOR");

		ModerationContentFilter.ApplyModerationUpdate(content, approved, filterState: ModerationContentFilter.AllStates);

		var stored = Assert.Single(content).Value;
		Assert.Same(approved, stored);
		Assert.Equal(ModerationState.Approved, stored.State);
		Assert.Equal("Reviewed", stored.Reason);
		Assert.Equal("MODERATOR", stored.Moderator);
	}

	[Fact]
	public void ShouldDisplay_ReturnsExpectedValuesForModerationFilters()
	{
		Assert.True(ModerationContentFilter.ShouldDisplay(ModerationContentFilter.AllStates, ModerationState.Pending));
		Assert.True(ModerationContentFilter.ShouldDisplay((int)ModerationState.Approved, ModerationState.Approved));
		Assert.False(ModerationContentFilter.ShouldDisplay((int)ModerationState.Approved, ModerationState.Rejected));
		Assert.False(ModerationContentFilter.ShouldDisplay((int)ModerationState.Pending, ModerationState.Approved));
	}

	private static ModerationContentModel CreateContent(
		ModerationState state,
		string providerId,
		string? reason = null,
		string? moderator = null) =>
		new(
			Provider: "TEST",
			ProviderId: providerId,
			Type: "Post",
			SourceUri: "https://example.com/post/" + providerId,
			Timestamp: DateTimeOffset.UtcNow,
			AuthorDisplayName: "Author",
			AuthorUserName: "@author",
			AuthorProfileUri: "https://example.com/author",
			AuthorProfileImageUri: "https://example.com/author.png",
			Text: "Hello world",
			PreviewCard: null,
			State: state,
			Reason: reason,
			Moderator: moderator,
			ModerationTimestamp: DateTimeOffset.UtcNow,
			Emotes: []);
}
