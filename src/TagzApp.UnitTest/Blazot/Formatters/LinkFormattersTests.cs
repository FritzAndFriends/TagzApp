using TagzApp.UnitTest.Blazot.TestData;

namespace TagzApp.UnitTest.Blazot.Formatters;

public class LinkFormattersTests
{
	[Theory]
	[ClassData(typeof(HashTagLinkData))]
	private void AddHashTagLinks_FormatsProperly_Test(string encodedText, string expectedText)
	{
		// Act
		var response = TagzApp.Providers.Blazot.Formatters.LinkFormatters.AddHashTagLinks(encodedText);

		// Assert
		Assert.Equal(expectedText, response);
	}
}
