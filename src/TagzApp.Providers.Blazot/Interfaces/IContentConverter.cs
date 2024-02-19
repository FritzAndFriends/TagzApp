using TagzApp.Providers.Blazot.Models;

namespace TagzApp.Providers.Blazot.Interfaces;

public interface IContentConverter
{
	IEnumerable<Content> ConvertToContent(List<Transmission>? transmissions, Hashtag tag);
}
