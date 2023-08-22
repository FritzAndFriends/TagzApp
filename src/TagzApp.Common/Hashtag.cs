namespace TagzApp.Common;

public class Hashtag
{
	private string _Text = string.Empty;

	public string Text
	{
		get { return _Text; }
		set
		{
			if (!value.StartsWith("#")) value = string.Concat("#", value);
			_Text = value;
		}
	}

	/// <summary>
	///   Clear all formatting around the text of a hashtag and make it lowercase.
	/// </summary>
	/// <param name="text">Hashtag text to clean up</param>
	/// <returns>Lowercase text of a hashtag with no formatting</returns>
	public static string ClearFormatting(string text)
	{
		return text.TrimStart('#').ToLowerInvariant();
	}
}