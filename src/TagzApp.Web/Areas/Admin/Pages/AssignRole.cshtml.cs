using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace TagzApp.Web.Areas.Admin.Pages;


public class AssignRoleModel : PageModel
{

	public string UserId { get; set; } // new property for user id
	public IdentityUser? CurrentUser { get; private set; }
	public List<string> UserRoles { get; private set; }
	public List<IdentityRole> Roles { get; private set; }

	private readonly UserManager<IdentityUser> _UserManager; // needed to access roles of a user
	private readonly RoleManager<IdentityRole> _RoleManager;

	public AssignRoleModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
	{
		_UserManager = userManager; // assign user manager to the private variable
		_RoleManager = roleManager;
	}

	public async Task OnGet(string userId) // using route parameter
	{
		UserId = userId; // assign user id to the new property

		CurrentUser = await _UserManager.FindByIdAsync(userId); // get user by id

		var roles = await _UserManager.GetRolesAsync(CurrentUser); // get roles assigned to the user

		UserRoles = new List<string>(roles); // store in a list

		Roles = await _RoleManager.Roles.ToListAsync(); // retrieve all roles in a list

	}
	public async Task<IActionResult> OnPostAsync(List<string> role, string userId)
	{
		var user = await _UserManager.FindByIdAsync(userId); // get user by id
		if (user == null)
		{
			return NotFound($"Unable to load user with ID '{userId}'.");
		}

		var userRoles = await _UserManager.GetRolesAsync(user); // get existing roles for user
		var result = await _UserManager.RemoveFromRolesAsync(user, userRoles); // remove existing roles

		if (!result.Succeeded)
		{
			ModelState.AddModelError("Error", "Failed to remove existing role(s)");
			return Page();
		}

		result = await _UserManager.AddToRolesAsync(user, role); // assign new roles
		if (!result.Succeeded)
		{
			ModelState.AddModelError("Error", "Failed to assign role(s)");
			return Page();
		}

		return RedirectToPage(new { userId = userId });
	}



}
