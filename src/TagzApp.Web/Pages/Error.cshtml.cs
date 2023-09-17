using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace TagzApp.Web.Pages
{
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	[IgnoreAntiforgeryToken]
	public class ErrorModel : PageModel
	{
		public string? RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

		private readonly ILogger<ErrorModel> _Logger;

		public ErrorModel(ILogger<ErrorModel> logger)
		{
			_Logger = logger;
		}

		public void OnGet()
		{
			RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
		}
	}
}
