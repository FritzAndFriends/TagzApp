// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagzApp.Web.Areas.Identity.Pages.Account.Manage
{
	public class ExternalLoginsModel : PageModel
	{
		private readonly UserManager<TagzAppUser> _userManager;
		private readonly SignInManager<TagzAppUser> _signInManager;
		private readonly IUserStore<TagzAppUser> _userStore;

		public ExternalLoginsModel(
				UserManager<TagzAppUser> userManager,
				SignInManager<TagzAppUser> signInManager,
				IUserStore<TagzAppUser> userStore)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_userStore = userStore;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public IList<UserLoginInfo> CurrentLogins { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public IList<AuthenticationScheme> OtherLogins { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public bool ShowRemoveButton { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[TempData]
		public string StatusMessage { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			CurrentLogins = await _userManager.GetLoginsAsync(user);
			OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
					.Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
					.ToList();

			string passwordHash = null;
			if (_userStore is IUserPasswordStore<TagzAppUser> userPasswordStore)
			{
				passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
			}

			ShowRemoveButton = passwordHash != null || CurrentLogins.Count > 1;
			return Page();
		}

		public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
			if (!result.Succeeded)
			{
				StatusMessage = "The external login was not removed.";
				return RedirectToPage();
			}

			await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, "Ticket Created");
			await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, "access_token");
			await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, "expires_at");
			await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, "token_type");
			await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, "Email");

			await _signInManager.RefreshSignInAsync(user);
			StatusMessage = "The external login was removed.";
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
		{
			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			// Request a redirect to the external login provider to link a login for the current user
			var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var userId = await _userManager.GetUserIdAsync(user);
			var info = await _signInManager.GetExternalLoginInfoAsync(userId);
			if (info == null)
			{
				throw new InvalidOperationException($"Unexpected error occurred loading external login info.");
			}

			var result = await _userManager.AddLoginAsync(user, info);
			if (!result.Succeeded)
			{
				StatusMessage = "The external login was not added. External logins can only be associated with one account.";
				return RedirectToPage();
			}

			await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			StatusMessage = "The external login was added.";
			return RedirectToPage();
		}
	}
}
