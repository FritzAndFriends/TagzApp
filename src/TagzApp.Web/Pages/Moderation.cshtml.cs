using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class ModerationModel : PageModel
{
	private readonly IMessagingService _Service;

	public ModerationModel(IMessagingService service)
	{
		_Service = service;
	}

	public List<string> Tags { get; } = new List<string>();

	public void OnGet()
	{

		Tags.AddRange(_Service.TagsTracked);


	}
}
