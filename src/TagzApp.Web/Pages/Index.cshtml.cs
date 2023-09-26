using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace TagzApp.Web.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		private readonly IMessagingService _Service;
		private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _HostingEnvironment;

		public IndexModel(ILogger<IndexModel> logger, IMessagingService service, IHostingEnvironment hostingEnvironment)
		{
			_logger = logger;
			_Service = service;
			_HostingEnvironment = hostingEnvironment;
		}

		public void OnGet()
		{

			if (_HostingEnvironment.IsDevelopment() || _HostingEnvironment.IsEnvironment("Test"))
			{

				foreach (var item in _Service.TagsTracked)
				{
					TagSearchModel.Tags.Add(item);
				}

			}

		}

		public IActionResult OnPost()
		{

			if ((_HostingEnvironment.IsDevelopment() || _HostingEnvironment.IsEnvironment("Test")) && !string.IsNullOrEmpty(TagSearchModel.NewTag) && !_Service.TagsTracked.Any())
			{
				_Service.AddHashtagToWatch(TagSearchModel.NewTag);
			}

			return RedirectToPage();

		}

		[BindProperty]
		public Shared.TagSearchModel TagSearchModel { get; set; } = new();


	}
}
