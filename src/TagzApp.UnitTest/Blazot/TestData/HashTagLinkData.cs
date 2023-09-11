using System.Collections;
using System.Web;

namespace TagzApp.UnitTest.Blazot.TestData;

public class HashTagLinkData : IEnumerable<object[]>
{
	private static readonly string _SimplePunctuationString = "I'm a test message";
	private static readonly string _HashTagTouching = "I'm a test message <br/>#dotnet";
	private static readonly string _TypicalString = "I'm a test message #dotnet";
	private static readonly string _EmoticonString = "I'm a test message 😊 #dotnet";

	private readonly List<object[]> _BodyText = new()
	{
		new object[] {HttpUtility.HtmlEncode(_SimplePunctuationString), "I&#39;m a test message"},
		new object[] {HttpUtility.HtmlEncode(_HashTagTouching), "I&#39;m a test message &lt;br/&gt;<a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a>" },
		new object[] {HttpUtility.HtmlEncode(_TypicalString), "I&#39;m a test message <a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a>" },
		new object[] {HttpUtility.HtmlEncode(_EmoticonString), "I&#39;m a test message &#128522; <a class=\"b-hashtag-link\" target=\"_blank\" href=\"https://blazot.com/hashtag/dotnet\">#dotnet</a>" }
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
