using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TagzApp.Storage.Postgres.ApplicationConfiguration;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class UiCustomizationModel : PageModel
	{
		private readonly ApplicationConfiguration _AppConfig;
		private readonly IApplicationConfigurationRepository _Repo;
		private readonly IConfigurationRoot _ConfigurationRoot;

		public UiCustomizationModel(
			IOptions<ApplicationConfiguration> appConfig,
			IApplicationConfigurationRepository repo,
			IConfiguration configurationRoot)
		{
			_AppConfig = appConfig.Value;
			_Repo = repo;
			_ConfigurationRoot = configurationRoot as IConfigurationRoot;
		}

		public void OnGet()
		{

			SiteName = _AppConfig.SiteName;
			WaterfallHeaderCss = _AppConfig.WaterfallHeaderCss;
			WaterfallHeaderMarkdown = _AppConfig.WaterfallHeaderMarkdown;

		}

		public IActionResult OnPost()
		{

			// Save the settings to the repository
			if (!ModelState.IsValid)
			{
				return Page();
			}

			_AppConfig.ForgetChanges();

			_AppConfig.SiteName = SiteName;
			_AppConfig.WaterfallHeaderCss = WaterfallHeaderCss;
			_AppConfig.WaterfallHeaderMarkdown = WaterfallHeaderMarkdown;

			_Repo.SetValues(_AppConfig);

			var thisProvider = _ConfigurationRoot.Providers.OfType<ApplicationConfigurationProvider>().First();
			thisProvider.Reload();

			return RedirectToPage("uicustomization", new { Area = "Admin" });

		}

		[BindProperty]
		public string SiteName { get; set; }

		[BindProperty]
		public string WaterfallHeaderCss { get; set; }

		[BindProperty]
		public string WaterfallHeaderMarkdown { get; set; }
	}
}
