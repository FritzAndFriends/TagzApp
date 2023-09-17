using TagzApp.Providers.Blazot.Models;

namespace TagzApp.Providers.Blazot.Interfaces;

internal interface IContentConverter
{
	IEnumerable<Content> ConvertToContent(List<Transmission>? transmissions, Hashtag tag);
}
