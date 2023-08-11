using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace TagzApp.Web.Areas.Admin.Pages;

public class UsersModel : PageModel
{
	private readonly UserManager<IdentityUser> _UserManager;

	public UsersModel(UserManager<IdentityUser> userManager)
  {
		_UserManager = userManager;
	}

  public List<IdentityUser> Users { get; set; }

	public void OnGet()
	{
		Users = _UserManager.Users.ToList();
	}
}
