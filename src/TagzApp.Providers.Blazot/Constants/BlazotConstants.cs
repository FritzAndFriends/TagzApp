namespace TagzApp.Providers.Blazot.Constants;

internal static class BlazotConstants
{
	/// <summary>
	/// The base address for the Blazot API.
	/// </summary>
	public const string BaseAddress = "https://api.blazot.com";

	/// <summary>
	/// The (non-api) base address for the Blazot website.
	/// </summary>
	public const string BaseAppAddress = "https://blazot.com";

	/// <summary>
	/// The provider id used in the database for provider system configuration.
	/// </summary>
	public const string ProviderId = "provider-blazot";

	/// <summary>
	/// The provider name used as an identifier in the database content entries.
	/// </summary>
	public const string Provider = "BLAZOT";

	/// <summary>
	/// The display name.
	/// </summary>
	public const string DisplayName = "Blazot";

	/// <summary>
	/// The provider description.
	/// </summary>
	public const string Description = "Interact with the Blazot social media service.";
}
