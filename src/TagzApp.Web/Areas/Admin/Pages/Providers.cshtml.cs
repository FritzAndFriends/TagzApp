using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class ProvidersModel : PageModel
	{
		public IEnumerable<ISocialMediaProvider> Providers { get; set; }
		private readonly IMessagingService _Service;

		public ProvidersModel(IMessagingService service)
		{
			_Service = service;
			Providers = new List<ISocialMediaProvider>();
		}

		public void OnGet()
		{
			Providers = _Service.Providers;
		}
	}
}
