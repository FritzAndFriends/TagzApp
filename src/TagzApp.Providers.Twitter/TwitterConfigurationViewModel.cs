using System.ComponentModel;
using TagzApp.Common.Attributes;

namespace TagzApp.Providers.Twitter;

public class TwitterConfigurationViewModel : IProviderConfigurationViewModel
{
	[DisplayName("URL")]
	[InputType("url")]
	public string BaseAddress { get; set; } = string.Empty;
	[InputType("timeout")]
	public string Timeout { get; set; } = string.Empty;
	[DisplayName("Default Headers")]
	public string DefaultHeaders { get; set; } = string.Empty;//TODO might need to figure out dictionary
	public string ApiKey { get; set; } = string.Empty;
	public string ApiSecretKey { get; set; } = string.Empty;
	public string AccessToken { get; set; } = string.Empty;
	public string AccessTokenSecret { get; set; } = string.Empty;
	public bool Activated { get; set; }
}
