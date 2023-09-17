using System.Text.RegularExpressions;
using System.Web;

namespace TagzApp.Providers.Blazot.Formatters;

internal static class LinkFormatters
{
	private static Regex LinkRegex => new(@"(?:(?:https?):\/\/)?[\w/\.-]+(?<!\.)(\.)(?!\.)[a-zA-Z]+(?<!\?)[a-zA-Z0-9/\-&?.=%#_]+(?<!\.)");
	private static Regex HashTagRegex => new(@"\B#\w\w+");
	private static Regex UserMentionRegex => new(@"\B@\w+");

	// This must be performed first, since it involves HTML Decoding and Encoding.
	public static string AddHashTagLinks(string bodyText)
	{
		bodyText = HttpUtility.HtmlDecode(bodyText);
		var matches = HashTagRegex.Matches(bodyText);
		bodyText = HttpUtility.HtmlEncode(bodyText);

		foreach (var match in matches)
		{
			if (match == null) continue;

			var m = match.ToString()!.Trim();
			var noHash = m.TrimStart('#');
			bodyText = bodyText.Replace(m, $"<a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/{noHash}\">{m}</a>");
		}

		return bodyText;
	}

	public static string AddWebLinks(string bodyText)
	{
		return LinkRegex.Replace(bodyText, delegate (Match m)
		{
			var fullLink = AddHttpIfMissing(m.Value);
			return $"<a class=\"b-url-link\" target=\"_blank\" href=\"{fullLink}\">{m.Value}</a>";
		});
	}

	public static string AddMentionLinks(string bodyText) =>
		UserMentionRegex.Replace(bodyText, m => $"<a class=\"b-url-link\" target=\"_blank\" href=\"https://blazot.com/{m.Value.Trim().TrimStart('@')}\">{m.Value}</a>");

	private static string AddHttpIfMissing(string url)
	{
		if (string.IsNullOrEmpty(url))
			return url;

		url = url.Trim();
		if (!url.StartsWith("http://") && !url.StartsWith("https://"))
			return $"http://{url}";

		return url;
	}
}
