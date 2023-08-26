using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class WaterfallModel : PageModel
{
	private readonly IMessagingService _Service;

	public WaterfallModel(IMessagingService service)
  {
		_Service = service;
	}

	public List<string> Tags { get; } = new List<string>();

	public void OnGet()
	{

		foreach (var item in _Service.TagsTracked)
		{
			Tags.Add(item);
		}

	}
}
