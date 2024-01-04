using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Humanizer;

namespace TagzApp.Web.Pages
{
	public class MessageDetailsModel : PageModel
	{
		private readonly IMessagingService _Service;
		private readonly IModerationRepository _ModerationRepository;
		private readonly UserManager<TagzAppUser> _UserManager;

		public MessageDetailsModel(IMessagingService service, IModerationRepository moderationRepository, UserManager<TagzAppUser> userManager)
		{
			_Service = service;
			_ModerationRepository = moderationRepository;
			_UserManager = userManager;
		}

		public (Content Content, ModerationAction Action) Message { get; set; }

		[HttpGet("MessageDetails/[id]")]
		public async Task OnGetAsync(string id)
		{

			var idParts = id.Split('|');

			if (idParts.Length != 2)
			{
				return;
			}

			var provider = idParts[0];
			var providerId = idParts[1];

			Message = await _ModerationRepository.GetContentWithModeration(provider, providerId);


		}

		[HttpPost("MessageDetails/[id]")]
		public async Task<IActionResult> OnPostAsync(string id, [FromForm] string blockUser)
		{

			if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(blockUser))
			{

				var idParts = id.Split('|');

				if (idParts.Length != 2)
				{
					return Page();
				}

				var provider = idParts[0];
				var providerId = idParts[1];
				var thisMessage = await _ModerationRepository.GetContentWithModeration(provider, providerId);

				var thisUser = await _UserManager.GetUserAsync(User);

				await _ModerationRepository.BlockUser(thisMessage.Content.Author.UserName, thisMessage.Content.Provider, thisUser.DisplayName, new DateTimeOffset(new DateTime(2050, 1, 1), TimeSpan.Zero));

				TempData["Message"] = $"Successfully blocked user {thisMessage.Content.Author.DisplayName} on {thisMessage.Content.Provider.ToLowerInvariant().Humanize(LetterCasing.Title)}";

				return RedirectToPage(new { id = id });

			}

			return Page();


		}

	}
}
