using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class UiCustomizationModel : PageModel
	{
		public void OnGet()
		{
		}

		public string SiteName { get; set; } = "TagzApp";

	}
}
