// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagzApp.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
	private readonly SignInManager<IdentityUser> _SignInManager;
	private readonly UserManager<IdentityUser> _UserManager;
	private readonly IUserStore<IdentityUser> _UserStore;
	private readonly IUserEmailStore<IdentityUser> _EmailStore;
	private readonly ILogger<RegisterModel> _Logger;
	private readonly IEmailSender _EmailSender;

	public RegisterModel(
		UserManager<IdentityUser> userManager,
		IUserStore<IdentityUser> userStore,
		SignInManager<IdentityUser> signInManager,
		ILogger<RegisterModel> logger,
		IEmailSender emailSender)
	{
		_UserManager = userManager;
		_UserStore = userStore;
		_EmailStore = GetEmailStore();
		_SignInManager = signInManager;
		_Logger = logger;
		_EmailSender = emailSender;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string ReturnUrl { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public IList<AuthenticationScheme> ExternalLogins { get; set; }

	public async Task OnGetAsync(string returnUrl = null)
	{
		ReturnUrl = returnUrl;
		ExternalLogins = (await _SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
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
			                                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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
