using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

		[Authorize]
		public async Task OnGetAsync()
		{

			var user = await _UserManager.GetUserAsync(User);
			var access_token = await _UserManager.GetAuthenticationTokenAsync(user, "Google", "access_token");

			Broadcasts = _Provider.GetBroadcastsForUser(access_token);

		}
	}
}
