// Ignore Spelling: Css

using System.ComponentModel.DataAnnotations;

namespace TagzApp.Common.Models;

public class ApplicationConfiguration
{

	public List<Settings> ChangedSettings = new();
	private string _SiteName = "TagzApp";
	private string _WaterfallHeaderMarkdown = "# Welcome to TagzApp";
	private string _WaterfallHeaderCss = string.Empty;

	[Required, MaxLength(30)]
	public string SiteName
	{
		get => _SiteName;
		set
		{
			_SiteName = value;
			ChangedSettings.Add(new Settings(SettingsKeys.SiteName, value));
		}
	}

	[Required]
	public string WaterfallHeaderMarkdown
	{
		get => _WaterfallHeaderMarkdown;
		set
		{
			_WaterfallHeaderMarkdown = value;
			ChangedSettings.Add(new Settings(SettingsKeys.WaterfallHeaderMarkdown, value));
		}
	}

	[Required]
	public string WaterfallHeaderCss
	{
		get => _WaterfallHeaderCss;
		set
		{
			_WaterfallHeaderCss = value;
			ChangedSettings.Add(new Settings(SettingsKeys.WaterfallHeaderCss, value));
		}
	}

	public static implicit operator Dictionary<string, string?>(ApplicationConfiguration config)
	{

		return new Dictionary<string, string?>
		{
			{ SettingsKeys.SiteName, config.SiteName },
			{ SettingsKeys.WaterfallHeaderMarkdown, config.WaterfallHeaderMarkdown },
			{ SettingsKeys.WaterfallHeaderCss, config.WaterfallHeaderCss }
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

	}

}


