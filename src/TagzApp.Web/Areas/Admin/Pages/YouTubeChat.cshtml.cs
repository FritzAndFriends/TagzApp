using System.Text.Json;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TagzApp.Communication;
using TagzApp.Providers.YouTubeChat;
using TagzApp.Storage.Postgres.ApplicationConfiguration;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class YouTubeChatModel : PageModel
	{
		private readonly IApplicationConfigurationRepository _Repository;
		private readonly ApplicationConfiguration _AppConfiguration;
		private readonly UserManager<TagzAppUser> _UserManager;
		private readonly IConfigurationRoot? _ConfigurationRoot;
		private YouTubeChatApplicationConfiguration? _YouTubeChatConfiguration;
		private YouTubeChatProvider _Provider;
		private IEnumerable<YouTubeBroadcast> _Broadcasts = Enumerable.Empty<YouTubeBroadcast>();

		public YouTubeChatModel(
			IMessagingService messagingService,
			IApplicationConfigurationRepository repository,
			IOptions<ApplicationConfiguration> appConfiguration,
			UserManager<TagzAppUser> userManager,
			IConfiguration configurationRoot)
		{

			var providers = (messagingService as BaseProviderManager).Providers;
			_Provider = providers.FirstOrDefault(p => p.Id == "YOUTUBE-CHAT") as YouTubeChatProvider;
			_Repository = repository;
			_AppConfiguration = appConfiguration.Value;
			_UserManager = userManager;
			_ConfigurationRoot = configurationRoot as IConfigurationRoot;


			if (!string.IsNullOrEmpty(_AppConfiguration.YouTubeChatConfiguration.Replace("{}", "")))
			{
				_YouTubeChatConfiguration = JsonSerializer.Deserialize<YouTubeChatApplicationConfiguration>(_AppConfiguration.YouTubeChatConfiguration);
				ChannelTitle = _YouTubeChatConfiguration.ChannelTitle;
				MonitoredChatId = _YouTubeChatConfiguration.LiveChatId;
			}
			else
			{
				_YouTubeChatConfiguration = new YouTubeChatApplicationConfiguration();
			}

		}

		public IEnumerable<YouTubeBroadcast> Broadcasts
		{
			get => _Broadcasts;
			set
			{
				_Broadcasts = value;
				TempData["Broadcasts"] = _Broadcasts;
			}
		}

		public string ChannelTitle { get; set; } = string.Empty;

		[BindProperty]
		public string MonitoredChatId { get; set; } = string.Empty;

		/// <summary>
		/// Get the RefreshToken and email configured for the Application
		/// </summary>
		/// <returns></returns>
		private async Task<(string, string)> IdentifyRefreshTokenAndEmail()
		{

			var refresh_token = _YouTubeChatConfiguration.RefreshToken;
			var email = _YouTubeChatConfiguration.ChannelEmail;

			if (string.IsNullOrEmpty(refresh_token) || string.IsNullOrEmpty(email))
			{

				var user = await _UserManager.GetUserAsync(User);
				refresh_token = await _UserManager.GetAuthenticationTokenAsync(user, "Google", "refresh_token");
				email = await _UserManager.GetAuthenticationTokenAsync(user, "Google", "Email");

			}

			return (refresh_token, email);

		}

		[Authorize]
		public async Task OnGetAsync()
		{

			MonitoredChatId = _Provider.LiveChatId;

			var (refresh_token, email) = await IdentifyRefreshTokenAndEmail();

			if (!string.IsNullOrEmpty(refresh_token) && !string.IsNullOrEmpty(email))
			{

				_Provider.YouTubeEmailId = email;
				_Provider.RefreshToken = refresh_token;

				try
				{
					ChannelTitle = !string.IsNullOrEmpty(_YouTubeChatConfiguration.ChannelTitle) ? _YouTubeChatConfiguration.ChannelTitle : await _Provider.GetChannelForUserAsync();
					base.TempData["ChannelTitle"] = ChannelTitle;
				}
				catch (TokenResponseException ex)
				{
					if (ex.Error.Error == "invalid_grant")
					{
						// do nothing, need to login again
						return;
					}

					throw;

				}

				Broadcasts = (await _Provider.GetBroadcastsForUser()).ToArray();

			}

		}

		[Authorize]
		public async Task<IActionResult> OnPostAsync()
		{

			var (refresh_token, email) = await IdentifyRefreshTokenAndEmail();
			ChannelTitle = TempData["ChannelTitle"] as string;
			var broadcasts = TempData["Broadcasts"] as IEnumerable<YouTubeBroadcast>;

			// do something with the value submitted
			_Provider.LiveChatId = MonitoredChatId;

			_YouTubeChatConfiguration.LiveChatId = MonitoredChatId;
			_YouTubeChatConfiguration.RefreshToken = refresh_token;
			_YouTubeChatConfiguration.ChannelEmail = email;
			_YouTubeChatConfiguration.ChannelTitle = ChannelTitle;
			_YouTubeChatConfiguration.BroadcastId = broadcasts?.FirstOrDefault(b => b.LiveChatId == MonitoredChatId)?.Id ?? string.Empty;
			_YouTubeChatConfiguration.BroadcastTitle = broadcasts?.FirstOrDefault(b => b.LiveChatId == MonitoredChatId)?.Title ?? string.Empty;

			_AppConfiguration.YouTubeChatConfiguration = JsonSerializer.Serialize(_YouTubeChatConfiguration);
			await _Repository.SetValues(_AppConfiguration);

			var thisProvider = _ConfigurationRoot.Providers.OfType<ApplicationConfigurationProvider>().First();
			thisProvider.Reload();

			return RedirectToPage("youtubechat", new { Area = "Admin" });

		}

	}
}
