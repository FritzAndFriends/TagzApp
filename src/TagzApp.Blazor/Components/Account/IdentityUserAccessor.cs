using Microsoft.AspNetCore.Identity;

namespace TagzApp.Blazor.Components.Account;
internal sealed class IdentityUserAccessor(UserManager<TagzAppUser> userManager, IdentityRedirectManager redirectManager)
{
	public async Task<TagzAppUser> GetRequiredUserAsync(HttpContext context)
	{
		var user = await userManager.GetUserAsync(context.User);

		if (user is null)
		{
			redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
		}

		return user;
	}
}
