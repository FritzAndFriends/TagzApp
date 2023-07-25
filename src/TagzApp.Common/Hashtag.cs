namespace TagzApp.Common;

public class Hashtag
{

	private string _Text = string.Empty;
  public string Text {
		get { return _Text; }
		set {
			if (!value.StartsWith("#")) value = string.Concat("#", value);
			_Text = value;
		}

	}

}
