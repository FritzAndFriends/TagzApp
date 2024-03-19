using Microsoft.AspNetCore.Components;
using TagzApp.Blazor.Client.Bootstrap;

namespace TagzApp.Blazor.Client.Services;

public record ToastMessage(string Message, MessageSeverity Severity = MessageSeverity.Normal, int Duration = 5000);

public class ToastService
{
	public event Action? OnUpdate;

	private List<ToastMessage> _Messages = new();
	public IReadOnlyList<ToastMessage> Messages => _Messages.AsReadOnly();

	private readonly NavigationManager _navManager;

	public ToastService(NavigationManager navManager)
	{
		_navManager = navManager;
		_navManager.LocationChanged += (_, _) => Clear();
	}

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

	private void Clear()
	{
		_Messages.Clear();
		OnUpdate?.Invoke();
	}
}
