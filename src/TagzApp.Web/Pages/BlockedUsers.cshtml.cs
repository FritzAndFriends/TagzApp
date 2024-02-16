using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagzApp.Web.Pages;

public class BlockedUsersModel : PageModel
{
	private readonly IMessagingService _Service;
	private readonly UserManager<TagzAppUser> _UserManager;
	private readonly IModerationRepository _Repository;

	public BlockedUsersModel(IMessagingService service, UserManager<TagzAppUser> userManager, IModerationRepository repository)
	{
		_Service = service;
		_UserManager = userManager;
		_Repository = repository;
	}

	public BlockedUser[] BlockedUsers { get; set; }

	public IEnumerable<ISocialMediaProvider> Providers { get; set; }

	[BindProperty]
	public string Provider { get; set; }

	[BindProperty]
	public string UserName { get; set; }

	public async Task<IActionResult> OnGetAsync()
	{

		// Get the current active providers
		Providers = _Service.Providers.OrderBy(x => x.DisplayName).ToArray();

		// Get the currently blocked users from the moderation repository
		BlockedUsers = (await _Repository.GetBlockedUsers()).ToArray();

		return Page();

	}

	// Add a Post method to block the user specified by Provider and UserName
	public async Task<IActionResult> OnPostAsync()
	{

		// Verify the user specified in Provider and UserName, then Block the user
		var thisUser = await _UserManager.GetUserAsync(User);
		await _Repository.BlockUser(UserName, Provider, thisUser.DisplayName ?? thisUser.UserName, new DateTimeOffset(new DateTime(2050, 1, 1), TimeSpan.Zero));

		return RedirectToPage();

	}

	// Add a handler for the unblock method
	public async Task<IActionResult> OnGetUnblockAsync(string provider, string userName)
	{

		// Verify the user specified in Provider and UserName, then Unblock the user
		var thisUser = await _UserManager.GetUserAsync(User);
		await _Repository.UnblockUser(userName, provider);

		return RedirectToPage();
	}


}
