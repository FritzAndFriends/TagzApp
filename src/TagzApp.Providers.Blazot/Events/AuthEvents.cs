namespace TagzApp.Providers.Blazot.Events;

internal interface IAuthEvents 
{
  event EventHandler AccessTokenUpdated;

  void NotifyAccessTokenUpdated();
}

internal class AuthEvents : IAuthEvents
{
  public event EventHandler AccessTokenUpdated;

  public void NotifyAccessTokenUpdated() => AccessTokenUpdated.Invoke(this, EventArgs.Empty);
}
