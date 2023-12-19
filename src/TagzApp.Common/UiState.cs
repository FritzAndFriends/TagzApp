namespace TagzApp.Common;
public class UiState
{

	private bool _FloatingHeader = false;
	public bool FloatingHeader
	{
		get { return _FloatingHeader; }
		set
		{
			_FloatingHeader = value;
			Console.WriteLine($"Setting FloatingHeader to {value}");
			UiUpdate?.Invoke(this, EventArgs.Empty);
		}
	}

	public event EventHandler UiUpdate;

}
