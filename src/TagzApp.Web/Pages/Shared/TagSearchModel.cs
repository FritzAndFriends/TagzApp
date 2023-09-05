using Microsoft.AspNetCore.Mvc;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages.Shared;

public class TagSearchModel
{

	public List<string> Tags { get; set; } = new();

	[BindProperty]
	public string NewTag { get; set; }


}
