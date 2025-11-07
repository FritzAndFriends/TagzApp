namespace TagzApp.Blazor.Services;

public class ConcurrentHashSet<T> : IDisposable
{
    private readonly HashSet<T> _hashSet = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public bool Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void Dispose()
    {
        _lock?.Dispose();
    }
}
