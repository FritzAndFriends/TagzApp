// Ignore Spelling: App Tagz

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class UiCustomizationModel : PageModel
	{

		private readonly ApplicationConfiguration _AppConfig;
		private readonly IConfigurationRoot _ConfigurationRoot;
		private readonly IConfigureTagzApp _ConfigureTagzApp;

		public UiCustomizationModel(
			IConfigureTagzApp configureTagzApp,
			ApplicationConfiguration appConfig,
			IConfiguration configurationRoot)
		{
			_AppConfig = appConfig;
			_ConfigurationRoot = configurationRoot as IConfigurationRoot;
			_ConfigureTagzApp = configureTagzApp;
		}

		public void OnGet()
		{

			ModerationEnabled = _AppConfig.ModerationEnabled;
			SiteName = _AppConfig.SiteName;
			WaterfallHeaderCss = _AppConfig.WaterfallHeaderCss;
			WaterfallHeaderMarkdown = _AppConfig.WaterfallHeaderMarkdown;

		}

		public async Task<IActionResult> OnPostAsync()
		{

			// Save the settings to the repository
			if (!ModelState.IsValid)
			{
				return Page();
			}

			_AppConfig.ModerationEnabled = ModerationEnabled;
			_AppConfig.SiteName = SiteName;
			_AppConfig.WaterfallHeaderCss = WaterfallHeaderCss;
			_AppConfig.WaterfallHeaderMarkdown = WaterfallHeaderMarkdown;

			await _AppConfig.SaveConfiguration(_ConfigureTagzApp);

			return RedirectToPage("uicustomization", new { Area = "Admin" });

		}

		[BindProperty, Display(AutoGenerateField = true, Name = "Moderation Enabled:")]
		public bool ModerationEnabled { get; set; }

		[BindProperty]
		public string SiteName { get; set; }

		[BindProperty]
		public string? WaterfallHeaderCss { get; set; }

		[BindProperty]
		public string WaterfallHeaderMarkdown { get; set; }
	}
}
