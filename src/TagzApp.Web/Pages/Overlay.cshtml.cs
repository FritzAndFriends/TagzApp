using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagzApp.Web.Pages;

public class OverlayModel : PageModel
{

	[FromRoute(Name = "tag")]
  public string Tag { get; set; }

  public void OnGet()
	{
	}
}
