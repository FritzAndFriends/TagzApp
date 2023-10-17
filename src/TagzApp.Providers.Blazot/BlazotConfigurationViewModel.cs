using System.ComponentModel;
using TagzApp.Common.Attributes;

namespace TagzApp.Providers.Blazot;

public class BlazotConfigurationViewModel : IProviderConfigurationViewModel
{
	[DisplayName("URL")]
	[InputType("url")]
	public string BaseAddress { get; set; } = string.Empty;
	[InputType("timeout")]
	public string Timeout { get; set; } = string.Empty;
	[DisplayName("API Key")]
	public string ApiKey { get; set; } = string.Empty;
	[DisplayName("Secret Auth Key")]
	public string SecretAuthKey { get; set; } = string.Empty;
	[DisplayName("Window Seconds")]
	public string WindowSeconds { get; set; } = string.Empty;
	[DisplayName("Window Requests")]
	public string WindowRequests { get; set; } = string.Empty;
	public bool Activated { get; set; }
}
