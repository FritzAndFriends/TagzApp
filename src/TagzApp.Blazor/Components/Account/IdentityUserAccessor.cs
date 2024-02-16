using Microsoft.AspNetCore.Identity;

namespace TagzApp.Blazor.Components.Account;
internal sealed class IdentityUserAccessor(UserManager<TagzAppUser> userManager, IdentityRedirectManager redirectManager)
{

	// private TagzAppUser _User;

	public async Task<TagzAppUser> GetRequiredUserAsync(HttpContext context)
	{

		if (context is null)
		{
			redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user.", context);
		}

		//if (_User is not null)
		//{
		//	return _User;
		//}

		var user = await userManager.GetUserAsync(context.User)!;

		if (user is null)
		{
			redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{context.User}'.", context);
		}

		return user;
	}

	//public async Task<TagzAppUser> GetRequiredUserAsync(AuthenticationState context)
	//{

	//	if (_User is not null)
	//	{
	//		return _User;
	//	}

	//	if (context is null)
	//	{
	//		redirectManager.RedirectTo("/Index");
	//	}

	//	_User = (await userManager.GetUserAsync(context.User))!;

	//	if (_User is null)
	//	{
	//		redirectManager.RedirectTo("/Index");
	//	}

	//	return _User;
	//}



}
