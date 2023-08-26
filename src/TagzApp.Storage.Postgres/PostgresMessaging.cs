namespace TagzApp.Storage.Postgres;

internal class PostgresMessaging : IDisposable
{
	private bool _DisposedValue;

	public PostgresMessaging()
	{
	}

	internal void StartProviders(IEnumerable<ISocialMediaProvider> providers, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	internal void SubscribeToContent(Hashtag hashtag, Action<Content> value)
	{
		throw new NotImplementedException();
	}

	#region Dispose Pattern Stuff

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~PostgresMessaging()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion

}