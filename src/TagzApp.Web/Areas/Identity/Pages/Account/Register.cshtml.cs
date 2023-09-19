// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Data;

namespace TagzApp.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
	private readonly SignInManager<TagzAppUser> _signInManager;
	private readonly UserManager<TagzAppUser> _userManager;
	private readonly IUserStore<TagzAppUser> _userStore;
	private readonly IUserEmailStore<TagzAppUser> _emailStore;
	private readonly ILogger<RegisterModel> _logger;
	private readonly IEmailSender _emailSender;

	public RegisterModel(
		UserManager<TagzAppUser> userManager,
		IUserStore<TagzAppUser> userStore,
		SignInManager<TagzAppUser> signInManager,
		ILogger<RegisterModel> logger,
		IEmailSender emailSender)
	{
		_userManager = userManager;
		_userStore = userStore;
		_emailStore = GetEmailStore();
		_signInManager = signInManager;
		_logger = logger;
		_emailSender = emailSender;
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
		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
	}

	private TagzAppUser CreateUser()
	{
		try
		{
			return Activator.CreateInstance<TagzAppUser>();
		}
		catch
		{
			throw new InvalidOperationException($"Can't create an instance of '{nameof(TagzAppUser)}'. " +
																					$"Ensure that '{nameof(TagzAppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
																					$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
		}
	}

	private IUserEmailStore<TagzAppUser> GetEmailStore()
	{
		if (!_userManager.SupportsUserEmail)
		{
			throw new NotSupportedException("The default UI requires a user store with email support.");
		}

		return (IUserEmailStore<TagzAppUser>)_userStore;
	}
}
