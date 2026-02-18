namespace TagzApp.Providers.LinkedIn;

/// <summary>
/// Internal cache model for resolved LinkedIn author profiles
/// </summary>
internal class LinkedInAuthor
{
	public required string PersonUrn { get; init; }
	public string DisplayName { get; init; } = string.Empty;
	public string VanityName { get; init; } = string.Empty;
	public string ProfileImageUrl { get; init; } = string.Empty;
	public DateTimeOffset CachedAt { get; init; } = DateTimeOffset.UtcNow;
}
