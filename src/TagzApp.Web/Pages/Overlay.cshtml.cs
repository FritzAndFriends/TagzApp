using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class OverlayModel : PageModel
{

	public OverlayModel(IMessagingService svc)
	{
		Tag = svc.TagsTracked.Any() ? svc.TagsTracked.First() : string.Empty;
	}

	public string Tag { get; }

	public void OnGet()
	{
	}
}
