// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace TagzApp.Web.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class ExternalLoginModel : PageModel
	{
		private readonly SignInManager<IdentityUser> _SignInManager;
		private readonly UserManager<IdentityUser> _UserManager;
		private readonly IUserStore<IdentityUser> _UserStore;
		private readonly IUserEmailStore<IdentityUser> _EmailStore;
		private readonly IEmailSender _EmailSender;
		private readonly ILogger<ExternalLoginModel> _Logger;

		public ExternalLoginModel(
				SignInManager<IdentityUser> signInManager,
				UserManager<IdentityUser> userManager,
				IUserStore<IdentityUser> userStore,
				ILogger<ExternalLoginModel> logger,
				IEmailSender emailSender)
		{
			_SignInManager = signInManager;
			_UserManager = userManager;
			_UserStore = userStore;
			_EmailStore = GetEmailStore();
			_Logger = logger;
			_EmailSender = emailSender;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		public InputModel Input { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public string ProviderDisplayName { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public string ReturnUrl { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[TempData]
		public string ErrorMessage { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public class InputModel
		{
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[EmailAddress]
			public string Email { get; set; }
		}

		public IActionResult OnGet() => RedirectToPage("./Login");

		public IActionResult OnPost(string provider, string returnUrl = null)
		{
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
			var properties = _SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");
			if (remoteError != null)
			{
				ErrorMessage = $"Error from external provider: {remoteError}";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}
			var info = await _SignInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				ErrorMessage = "Error loading external login information.";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}

			// Sign in the user with this external login provider if the user already has a login.
			var result = await _SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			if (result.Succeeded)
			{
				_Logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
				return LocalRedirect(returnUrl);
			}
			if (result.IsLockedOut)
			{
				return RedirectToPage("./Lockout");
			}
			else
			{
				// If the user does not have an account, then ask the user to create an account.
				ReturnUrl = returnUrl;
				ProviderDisplayName = info.ProviderDisplayName;
				if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
				{
					Input = new InputModel
					{
						Email = info.Principal.FindFirstValue(ClaimTypes.Email)
					};
				}
				return Page();
			}
		}

		public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");
			// Get the information about the user from the external login provider
			var info = await _SignInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				ErrorMessage = "Error loading external login information during confirmation.";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}

			if (ModelState.IsValid)
			{
				var user = CreateUser();

				await _UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
				await _EmailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

				var result = await _UserManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await _UserManager.AddLoginAsync(user, info);
					if (result.Succeeded)
					{
						_Logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

						await AssignAdminForFirstUser();

						var userId = await _UserManager.GetUserIdAsync(user);
						var code = await _UserManager.GenerateEmailConfirmationTokenAsync(user);
						code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
						var callbackUrl = Url.Page(
								"/Account/ConfirmEmail",
								pageHandler: null,
								values: new { area = "Identity", userId = userId, code = code },
								protocol: Request.Scheme);

						await _EmailSender.SendEmailAsync(Input.Email, "Confirm your email",
								$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

						// If account confirmation is required, we need to show the link if we don't have a real email sender
						if (_UserManager.Options.SignIn.RequireConfirmedAccount)
						{
							return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
						}


						await _SignInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
						return LocalRedirect(returnUrl);
					}
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			ProviderDisplayName = info.ProviderDisplayName;
			ReturnUrl = returnUrl;
			return Page();
		}

		private async Task AssignAdminForFirstUser()
		{

			if (_UserManager.Users.Count() == 1)
			{

				var user = _UserManager.Users.First();
				await _UserManager.AddToRoleAsync(user, Security.Role.Admin);

			}

		}

		private IdentityUser CreateUser()
		{
			try
			{
				return Activator.CreateInstance<IdentityUser>();
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
						$"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
						$"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
			}
		}

		private IUserEmailStore<IdentityUser> GetEmailStore()
		{
			if (!_UserManager.SupportsUserEmail)
			{
				throw new NotSupportedException("The default UI requires a user store with email support.");
			}
			return (IUserEmailStore<IdentityUser>)_UserStore;
		}
	}
}
