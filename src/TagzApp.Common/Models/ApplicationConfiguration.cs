// Ignore Spelling: Css

using System.ComponentModel.DataAnnotations;

namespace TagzApp.Common.Models;

public class ApplicationConfiguration
{
	private const string CONFIG_KEY = "ApplicationConfiguration";
	private string _SiteName = "TagzApp";
	private string _WaterfallHeaderMarkdown = "# Welcome to TagzApp";
	private string _WaterfallHeaderCss = string.Empty;
	private bool _ModerationEnabled = false;

	private string _YouTubeChatConfig = "{}";

	public bool ModerationEnabled
	{
		get => _ModerationEnabled;
		set
		{
			if (_ModerationEnabled == value) return;
			_ModerationEnabled = value;
		}
	}

	[Required, MaxLength(30)]
	public string SiteName
	{
		get => _SiteName;
		set
		{
			if (_SiteName == value) return;
			_SiteName = value;
		}
	}

	[Required]
	public string WaterfallHeaderMarkdown
	{
		get => _WaterfallHeaderMarkdown;
		set
		{
			if (_WaterfallHeaderMarkdown == value) return;
			_WaterfallHeaderMarkdown = value;
		}
	}

	[Required]
	public string WaterfallHeaderCss
	{
		get => _WaterfallHeaderCss;
		set
		{
			if (_WaterfallHeaderCss == value) return;
			_WaterfallHeaderCss = value;
		}
	}

	public string YouTubeChatConfiguration
	{
		get { return _YouTubeChatConfig; }
		set
		{
			if (_YouTubeChatConfig == value) return;
			_YouTubeChatConfig = value;
		}
	}

	public static async Task<ApplicationConfiguration> LoadFromConfiguration(IConfigureTagzApp config)
	{

		return await config.GetConfigurationById<ApplicationConfiguration>(CONFIG_KEY);

	}

	public async Task SaveConfiguration(IConfigureTagzApp configure)
	{

		await configure.SetConfigurationById(CONFIG_KEY, this);

	}

}
