using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Communication;
using TagzApp.Providers.YouTubeChat;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class YouTubeChatModel : PageModel
	{
		private readonly SignInManager<TagzAppUser> _SignInManager;
		private readonly UserManager<TagzAppUser> _UserManager;
		private YouTubeChatProvider _Provider;

		public YouTubeChatModel(IMessagingService messagingService, SignInManager<TagzAppUser> signInManager, UserManager<TagzAppUser> userManager)
		{

			var providers = (messagingService as BaseProviderManager).Providers;
			_Provider = providers.FirstOrDefault(p => p.Id == "YOUTUBE-CHAT") as YouTubeChatProvider;
			_SignInManager = signInManager;
			_UserManager = userManager;
		}

		public IEnumerable<YouTubeBroadcast> Broadcasts { get; set; }

		public string ChannelTitle { get; set; }

		[BindProperty]
		public string MonitoredChatId { get; set; } = string.Empty;

		[Authorize]
		public async Task OnGetAsync()
		{

			MonitoredChatId = _Provider.LiveChatId;

			var user = await _UserManager.GetUserAsync(User);
			var refresh_token = await _UserManager.GetAuthenticationTokenAsync(user, "Google", "refresh_token");
			var email = await _UserManager.GetAuthenticationTokenAsync(user, "Google", "Email");

			_Provider.YouTubeEmailId = email;
			_Provider.RefreshToken = refresh_token;

			// TODO: Error handle if there is no access token
			// TODO: Handle a shared access token definition for the system

			ChannelTitle = await _Provider.GetChannelForUserAsync();

			Broadcasts = await _Provider.GetBroadcastsForUser();

		}

		[Authorize]
		public async Task<IActionResult> OnPostAsync()
		{

			// do something with the value submitted
			_Provider.LiveChatId = MonitoredChatId;

			// _ = _Provider.GetContentForHashtag(null, DateTimeOffset.UtcNow);

			return RedirectToPage("youtubechat", new { Area = "Admin" });

		}

	}
}
