namespace TagzApp.Common.Models;

/// <summary>
///   A card demonstrating image content associated with a message
/// </summary>
public class Card
{
	public required Uri ImageUri { get; init; }

	public string AltText { get; init; } = string.Empty;

	public int Height { get; init; }

	public int Width { get; init; }
}
