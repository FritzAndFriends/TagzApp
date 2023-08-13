using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class OverlayModel : PageModel
{

	public OverlayModel(InMemoryMessagingService svc)
  {
		Tag = svc.Content.Any() ? svc.Content.Keys.First() : string.Empty;
	}

  public string Tag { get; }

  public void OnGet()
	{
	}
}
