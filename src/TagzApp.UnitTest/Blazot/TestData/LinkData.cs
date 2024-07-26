using System.Collections;
using System.Web;

namespace TagzApp.UnitTest.Blazot.TestData;

public class LinkData : IEnumerable<object[]>
{
	private static readonly string _LinkString = "Blazot - your launchpad to the social universe - https://blazot.com";
	private static readonly string _StartLinkString = "https://blazot.com - Link at very beginning";
	private static readonly string _LinkHashtagComboString = "#dotnet https://blazot.com";
	private static readonly string _LinkHashtagComboString2 = "Just a test. #dotnet https://blazot.com";
	private static readonly string _LinkWithHashSymbol = "Testing. https://github.com/dotnet/aspnetcore/issues/51046#issuecomment-2034933362";
	private static readonly string _LinkWithQueryParam = "Testing. https://www.youtube.com/live/ubX-a6_V_ao?si=CesC5nZrAI5quccu";
	private static readonly string _LinkWithQueryParam2 = "Testing. https://www.youtube.com/live/hrXAkNsjaoI?feature=shared&t=541";
	private static readonly string _SimplePunctuationString = "I'm a test message";
	private static readonly string _HashTagTouching = "I'm a test message <br/>#dotnet";
	private static readonly string _TypicalString = "I'm a test message #dotnet";
	private static readonly string _EmoticonString = "I'm a test message 😊 #dotnet";
	private static readonly string _MentionString = "@csharpfritz loves C#.";
	private static readonly string _HtmlString = "<b>test</b>";


	private readonly List<object[]> _BodyText = new()
	{
		new object[] {HttpUtility.HtmlEncode(_LinkString), "Blazot - your launchpad to the social universe - <a class=\"b-url-link\" target=\"_blank\" href=\"https://blazot.com\">https://blazot.com</a>" },
		new object[] {HttpUtility.HtmlEncode(_StartLinkString), "<a class=\"b-url-link\" target=\"_blank\" href=\"https://blazot.com\">https://blazot.com</a> - Link at very beginning" },
		new object[] {HttpUtility.HtmlEncode(_LinkHashtagComboString), "<a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a> <a class=\"b-url-link\" target=\"_blank\" href=\"https://blazot.com\">https://blazot.com</a>" },
		new object[] {HttpUtility.HtmlEncode(_LinkHashtagComboString2), "Just a test. <a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a> <a class=\"b-url-link\" target=\"_blank\" href=\"https://blazot.com\">https://blazot.com</a>" },
		new object[] {HttpUtility.HtmlEncode(_LinkWithHashSymbol), "Testing. <a class=\"b-url-link\" target=\"_blank\" href=\"https://github.com/dotnet/aspnetcore/issues/51046#issuecomment-2034933362\">https://github.com/dotnet/aspnetcore/issues/51046#issuecomment-2034933362</a>" },
		new object[] {HttpUtility.HtmlEncode(_LinkWithQueryParam), "Testing. <a class=\"b-url-link\" target=\"_blank\" href=\"https://www.youtube.com/live/ubX-a6_V_ao?si=CesC5nZrAI5quccu\">https://www.youtube.com/live/ubX-a6_V_ao?si=CesC5nZrAI5quccu</a>" },
		new object[] {HttpUtility.HtmlEncode(_LinkWithQueryParam2), "Testing. <a class=\"b-url-link\" target=\"_blank\" href=\"https://www.youtube.com/live/hrXAkNsjaoI?feature=shared&t=541\">https://www.youtube.com/live/hrXAkNsjaoI?feature=shared&t=541</a>" },
		new object[] {HttpUtility.HtmlEncode(_SimplePunctuationString), "I&#39;m a test message"},
		new object[] {HttpUtility.HtmlEncode(_HashTagTouching), "I&#39;m a test message &lt;br/&gt;<a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a>" },
		new object[] {HttpUtility.HtmlEncode(_TypicalString), "I&#39;m a test message <a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a>" },
		new object[] {HttpUtility.HtmlEncode(_EmoticonString), "I&#39;m a test message &#128522; <a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a>" },
		new object[] {HttpUtility.HtmlEncode(_MentionString), "<a class=\"b-url-link\" target=\"_blank\" href=\"https://blazot.com/csharpfritz\">@csharpfritz</a> loves C#." },
		new object[] {HttpUtility.HtmlEncode(_HtmlString), "&lt;b&gt;test&lt;/b&gt;" }
	};

	public IEnumerator<object[]> GetEnumerator()
	{
		return _BodyText.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
