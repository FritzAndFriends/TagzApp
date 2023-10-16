using System.ComponentModel;
using TagzApp.Common.Attributes;

namespace TagzApp.Providers.Mastodon;

public class MastodonConfigurationViewModel : IProviderConfigurationViewModel
{
	[DisplayName("URL")]
	[InputType("url")]
	public string BaseAddress { get; set; } = string.Empty;
	[InputType("timeout")]
	public string Timeout { get; set; } = string.Empty;
	[DisplayName("Default Headers")]
	public string DefaultHeaders { get; set; } = string.Empty;
	[DisplayName("Use HTTP/2")]
	public bool UseHttp2 { get; set; }
	public bool Activated { get; set; }
}
