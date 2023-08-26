using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		public readonly IConfiguration _Configuration;
		private readonly IMessagingService _Service;

		public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, IMessagingService service)
		{
			_logger = logger;
			_Configuration = configuration;
			_Service = service;
		}

		public void OnGet()
		{

			foreach (var item in _Service.TagsTracked)
			{
				Tags.Add(item);
			}

		}

		public IActionResult OnPost()
		{

			if (!string.IsNullOrEmpty(NewTag))
			{
				_Service.AddHashtagToWatch(NewTag);
			}

			return RedirectToPage();

		}

		[BindProperty]
		public string NewTag { get; set; }

		public List<string> Tags { get; } = new List<string>();

	}
}