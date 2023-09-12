using System.ComponentModel;

namespace TagzApp.Providers.Mastodon;

public class MastodonConfigurationViewModel : IProviderConfigurationViewModel
{
	[DisplayName("URL")]
	public string BaseAddress { get; set; } = string.Empty;
	public string Timeout { get; set; } = string.Empty;
	public string DefaultHeaders { get; set; } = string.Empty;//TODO might need to figure out dictionary 
	public bool UseHttp2 { get; set; }
	public bool Activated { get; set; }
}
