using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagzApp.Web.Pages
{
	public class PrivacyModel : PageModel
	{
		private readonly ILogger<PrivacyModel> _Logger;

		public PrivacyModel(ILogger<PrivacyModel> logger)
		{
			_Logger = logger;
		}

		public void OnGet()
		{
		}
	}
}
