using TagzApp.Blazor.Bootstrap;

namespace TagzApp.Blazor.Services;

public record ToastMessage(string Message, MessageSeverity Severity = MessageSeverity.Normal, int Duration=5000);

public class ToastService
{
	public event Action? OnUpdate;

	private List<ToastMessage> _Messages = new();
	public IReadOnlyList<ToastMessage> Messages => _Messages.AsReadOnly();

	public void Add(string message, MessageSeverity severity = MessageSeverity.Normal)
	{
		_Messages.Add(new(message, severity));
		OnUpdate?.Invoke();
	}

	public void Remove(ToastMessage message)
	{
		_Messages.Remove(message);
		OnUpdate?.Invoke();
	}
}
