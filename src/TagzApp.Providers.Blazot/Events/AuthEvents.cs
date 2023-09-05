namespace TagzApp.Providers.Blazot.Events;

internal interface IAuthEvents
{
	event EventHandler AccessTokenUpdated;

	void NotifyAccessTokenUpdated();
}

internal class AuthEvents : IAuthEvents
{
	// TODO: Check CS8618: Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public event EventHandler AccessTokenUpdated;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public void NotifyAccessTokenUpdated() => AccessTokenUpdated.Invoke(this, EventArgs.Empty);
}
