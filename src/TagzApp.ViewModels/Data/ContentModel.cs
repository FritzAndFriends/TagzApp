using System.Net;

namespace TagzApp.ViewModels.Data;

/// <summary>
/// Content to be shared with the web client
/// </summary>
/// <param name="Provider">The provider the content was identified on</param>
/// <param name="Type">The type of content identified</param>
/// <param name="SourceUri">Source URL of the content</param>
/// <param name="Timestamp">When the content was created</param>
/// <param name="AuthorDisplayName">Display name of the author of the content</param>
/// <param name="AuthorProfileUri">Profile URI of the author of the content</param>
/// <param name="AuthorProfileImageUri">Profile Image URI of the author of the content</param>
/// <param name="Text">Text of the content</param>
public record ContentModel(
	string Provider,
	string ProviderId,
	string Type,
	string SourceUri,
	DateTimeOffset Timestamp,
	string AuthorDisplayName,
	string AuthorUserName,
	string AuthorProfileUri,
	string AuthorProfileImageUri,
	string Text,
	Card? PreviewCard,
	Emote[] Emotes
)
{

	// TODO: Refactor to not take a dependency on the Common library
	public static explicit operator ContentModel(Content content)
	{
		return new ContentModel(
			content.Provider,
			content.ProviderId,
			content.Type.ToString(),
			content.SourceUri.ToString(),
			content.Timestamp,
			content.Author.DisplayName,
			content.Author.UserName,
			content.Author.ProfileUri.ToString(),
			content.Author.ProfileImageUri.ToString(),
			content.Text,
			content.PreviewCard,
			content.Emotes ?? new Emote[0]
		);
	}

	public string FormatContentWithEmotes()
	{

		if (!Emotes?.Any() ?? true)
		{
			return Text;
		}

		var originalText = WebUtility.HtmlDecode(Text);
		var formattedContent = originalText;

		var toReplace = new List<EmoteFormat>();
		foreach (var emote in Emotes)
		{

			var emoteUrl = emote.ImageUrl;

			var emoteName = originalText
				.Substring(emote.Pos, emote.Length)
				.Trim();
			var emoteHtml = $"""<img class="emote" src="{emoteUrl}"  />""";
			toReplace.Add(new EmoteFormat(emoteName, emoteHtml));
		}

		foreach (var r in toReplace)
		{
			formattedContent = formattedContent.Replace(r.Name, r.HTML);
		}

		return formattedContent;

	}

	public static string MapProviderToIcon(string provider) =>
		provider?.ToLowerInvariant().Trim() switch
		{
			"blazot" => "bi-blazot",
			"bluesky" => "icon-bluesky",
			"twitter" => "bi-twitter-x",
			"website" => "bi-globe2",
			"youtube-chat" => "bi-youtube",
			_ => $"bi-{provider?.ToLowerInvariant().Trim() ?? "question-circle"}"
		};



}

internal record EmoteFormat(string Name, string HTML);
