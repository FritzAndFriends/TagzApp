using System.Text.RegularExpressions;

namespace TagzApp.Providers.Blazot.Formatters;

internal static class LinkFormatters
{
  private static Regex LinkRegex => new(@"(?:(?:https?):\/\/)?[\w/\.-]+(?<!\.)(\.)(?!\.)[a-zA-Z]+(?<!\?)[a-zA-Z0-9/\-&?.=%#_]+(?<!\.)");
  private static Regex HashTagRegex => new(@"(^|[^&\p{L}\p{M}\p{Nd}_\u200c\u200d\ua67e\u05be\u05f3\u05f4\u309b\u309c\u30a0\u30fb\u3003\u0f0b\u0f0c\u00b7])(#|\uFF03)(?!\uFE0F|\u20E3)([\p{L}\p{M}\p{Nd}_\u200c\u200d\ua67e\u05be\u05f3\u05f4\u309b\u309c\u30a0\u30fb\u3003\u0f0b\u0f0c\u00b7]*[\p{L}\p{M}][\p{L}\p{M}\p{Nd}_\u200c\u200d\ua67e\u05be\u05f3\u05f4\u309b\u309c\u30a0\u30fb\u3003\u0f0b\u0f0c\u00b7]*)");
  private static Regex UserMentionRegex => new(@"\B@\w+");

  public static string AddHashTagLinks(string bodyText)
  {
    return HashTagRegex.Replace(bodyText, delegate(Match m)
    {
      var noHash = m.Value.Trim().TrimStart('#');
      var hashed = m.Value.Trim();
      return $" <a class=\"b-hashtag-link\" href=\"https://blazot.com/hashtag/{noHash}\">{hashed}</a>";
    });
  }

  public static string AddWebLinks(string bodyText)
  {
    return LinkRegex.Replace(bodyText, delegate(Match m)
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
