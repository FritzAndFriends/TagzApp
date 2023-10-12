using TagzApp.UnitTest.Blazot.TestData;

namespace TagzApp.UnitTest.Blazot.Formatters;

public class LinkFormattersTests
{
	[Theory]
	[ClassData(typeof(LinkData))]
	private void WebLinksFormatProperly_Test(string sourceText, string expectedText)
	{
		// Act
		var response = TagzApp.Providers.Blazot.Formatters.LinkFormatters.FormatBodyLinks(sourceText);

		// Assert
		Assert.Equal(expectedText, response);
	}
}
