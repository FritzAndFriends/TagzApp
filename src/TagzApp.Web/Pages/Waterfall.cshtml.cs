using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class WaterfallModel : PageModel
{
	private readonly InMemoryMessagingService _Service;

	public WaterfallModel(InMemoryMessagingService service)
  {
		_Service = service;
	}

	public List<string> Tags { get; } = new List<string>();

	public void OnGet()
	{

		foreach (var item in _Service.Content)
		{
			Tags.Add(item.Key);
		}

	}
}
