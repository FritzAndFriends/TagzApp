using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Data;

namespace TagzApp.Web.Areas.Admin.Pages;

public class UsersModel : PageModel
{
	private readonly UserManager<TagzAppUser> _UserManager;

	public UsersModel(UserManager<TagzAppUser> userManager)
	{
		_UserManager = userManager;
	}

	public List<TagzAppUser> Users { get; set; }

	public void OnGet()
	{
		Users = _UserManager.Users.ToList();
	}
}
