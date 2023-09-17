using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Pages.Shared;
using TagzApp.Web.Services;

namespace TagzApp.Web.Areas.Admin.Pages;

public class IndexModel : PageModel
{
	protected readonly IMessagingService _Service;

	public IndexModel(IMessagingService service)
	{
		_Service = service;
	}

	public void OnGet()
	{

		foreach (var item in _Service.TagsTracked)
		{
			TagSearchModel.Tags.Add(item);
		}

	}

	public IActionResult OnPost()
	{

		if (!string.IsNullOrEmpty(TagSearchModel.NewTag) && !_Service.TagsTracked.Any())
		{
			_Service.AddHashtagToWatch(TagSearchModel.NewTag);
		}

		return RedirectToPage();

	}

	[BindProperty]
	public TagSearchModel TagSearchModel { get; set; } = new();


}
