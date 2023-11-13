// Ignore Spelling: Css

using System.ComponentModel.DataAnnotations;

namespace TagzApp.Common.Models;

public class ApplicationConfiguration
{

	public List<Settings> ChangedSettings = new();
	private string _SiteName = "TagzApp";
	private string _WaterfallHeaderMarkdown = "# Welcome to TagzApp";
	private string _WaterfallHeaderCss = string.Empty;

	private string _YouTubeChatConfig = "{}";

	[Required, MaxLength(30)]
	public string SiteName
	{
		get => _SiteName;
		set
		{
			if (_SiteName == value) return;
			_SiteName = value;
			ChangedSettings.RemoveAll(s => s.Id == SettingsKeys.SiteName);
			ChangedSettings.Add(new Settings(SettingsKeys.SiteName, value));
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
			ChangedSettings.RemoveAll(s => s.Id == SettingsKeys.WaterfallHeaderMarkdown);
			ChangedSettings.Add(new Settings(SettingsKeys.WaterfallHeaderMarkdown, value));
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
			ChangedSettings.RemoveAll(s => s.Id == SettingsKeys.WaterfallHeaderCss);
			ChangedSettings.Add(new Settings(SettingsKeys.WaterfallHeaderCss, value));
		}
	}

	public string YouTubeChatConfiguration
	{
		get { return _YouTubeChatConfig; }
		set
		{
			if (_YouTubeChatConfig == value) return;
			_YouTubeChatConfig = value;
			ChangedSettings.RemoveAll(s => s.Id == SettingsKeys.YouTubeChatConfiguration);
			ChangedSettings.Add(new Settings(SettingsKeys.YouTubeChatConfiguration, value));
		}
	}

	public static implicit operator Dictionary<string, string?>(ApplicationConfiguration config)
	{

		return new Dictionary<string, string?>
		{
			{ SettingsKeys.SiteName, config.SiteName },
			{ SettingsKeys.WaterfallHeaderMarkdown, config.WaterfallHeaderMarkdown },
			{ SettingsKeys.WaterfallHeaderCss, config.WaterfallHeaderCss },
			{ SettingsKeys.YouTubeChatConfiguration, config.YouTubeChatConfiguration }
		};

	}

	public void ForgetChanges()
	{
		ChangedSettings.Clear();
	}

	private class SettingsKeys
	{

		public const string SiteName = "ApplicationConfiguration:SiteName";
		public const string WaterfallHeaderMarkdown = "ApplicationConfiguration:WaterfallHeaderMarkdown";
		public const string WaterfallHeaderCss = "ApplicationConfiguration:WaterfallHeaderCss";
		public const string YouTubeChatConfiguration = "ApplicationConfiguration:YouTubeChatConfiguration";

	}

}


