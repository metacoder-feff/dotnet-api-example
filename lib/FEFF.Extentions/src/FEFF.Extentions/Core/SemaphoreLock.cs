namespace FEFF.Extentions;

/// <summary>
/// AsyncLock using 'SemaphoreSlim'.</br>
/// Does not throw at SemaphoreSlim.Release() when is disposed.</br>
/// Does not wait for SemaphoreSlim.Release() at .Dispose().
/// </summary>
/// <remarks>
/// NOT reentrant!!! <br/>
/// The nearest realization is DotNext.Threading.AsyncLock.Semaphore 
/// but it throws at SemaphoreSlim.Release() 
/// and waits (may deadlock)  at AsyncLock.DisposeAsync().
/// </remarks>
public sealed class SemaphoreLock: IDisposable
{
    private readonly SemaphoreSlim _semaphore;// = new(ParrallelDBTests); // max locks on semaphore 

    /// <summary>
    /// Gets the current count of the <see cref="SemaphoreSlim"/>.
    /// </summary>
    /// <value>The current count of the <see cref="SemaphoreSlim"/>.</value>
    public int CurrentCount => _semaphore.CurrentCount;

    public SemaphoreLock(int maxParallelism = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxParallelism, 0);

        // we do not need to set 'maxCount'
        // because we do not allow 'Release()' without 'Wait()'
        // via encapsulation
        _semaphore = new (maxParallelism);
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<Handler> EnterAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new Handler(_semaphore);
    }

    public async Task<Handler?> TryEnterAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var b = await _semaphore.WaitAsync(timeout, cancellationToken);
        if(b == false)
            return null;

        return new Handler(_semaphore);
    }

    public sealed class Handler: IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        // threadsafe bool, interlocked
        private volatile int _isDisposed = 0; //false;

        public Handler(SemaphoreSlim semaphore)
        {
            ArgumentNullException.ThrowIfNull(semaphore);
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            // set _isDisposed = true;
            // and get prev value
            var hasAlreadyBeenDisposed = Interlocked.Exchange(ref _isDisposed, 1);

            if (hasAlreadyBeenDisposed > 0)
                return;

            try
            {
                _semaphore.Release();
            }
            catch(ObjectDisposedException)
            {
                // The aim of 'SemaphoreLock' is to avoid concurrent access to a resource.
                // There is no matter to throw at Release() because Lock would never be entered again
                // when SemaphoreLock (=SemaphoreSlim) is disposed
            }

            GC.SuppressFinalize(this);
        }
    }
}