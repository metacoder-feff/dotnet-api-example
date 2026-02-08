namespace FEFF.Extentions;

/*
The nearest realization is DotNext.Threading.AsyncLock: Semaphore/Exclusive.
But it throws at Release() when AsyncLock is disposed
and waits for all 'enters' to be released (may deadlock) at AsyncLock.DisposeAsync().
*/

//TODO: Think about reentrant realization based on AsyncLocal

/*
HINT: 
Do not implement 'auto dispose _semaphore after last release' feature beacuse 
we want to throw at EnterAsync()/TryEnterAsync when SemaphoreLock is disposed
to prevent access to shared resource without succesfull lock.
*/

/// <summary>
/// AsyncLock using 'SemaphoreSlim'.</br>
/// Does not throw at SemaphoreSlim.Release() when is disposed.</br>
/// Does not wait for SemaphoreSlim.Release() at SemaphoreLock.Dispose().
/// </summary>
/// <remarks>
/// NOT reentrant!!! <br/>
/// Throws at EnterAsync()/TryEnterAsync when SemaphoreLock is disposed.
/// </remarks>
public sealed class SemaphoreLock: IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Gets the current count of the <see cref="SemaphoreSlim"/>.
    /// </summary>
    /// <value>The current count of the <see cref="SemaphoreSlim"/>.</value>
    public int CurrentCount => _semaphore.CurrentCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="SemaphoreLock"/> class, specifying
    /// the number of requests that can be granted concurrently.
    /// </summary>
    /// <param name="maxParallelism">The number of requests for the semaphore that can be granted
    /// concurrently.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxParallelism"/>
    /// is less than 1.</exception>
    public SemaphoreLock(int maxParallelism = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxParallelism, 1);

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

    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreLock"/>, while observing a
    /// <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> token to observe.
    /// </param>
    /// <returns>A task returning a 'disposable' to be used for releasing the lock.</returns>
    /// <exception cref="ObjectDisposedException">
    /// If the <see cref="SemaphoreLock"/> is disposed.
    /// </exception>
    public async Task<IDisposable> EnterAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Releaser(_semaphore);
    }
    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreLock"/>, using a <see
    /// cref="TimeSpan"/> to measure the time interval.
    /// </summary>
    /// <param name="timeout">
    /// A <see cref="TimeSpan"/> that represents the number of milliseconds
    /// to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> token to observe.
    /// </param>
    /// <returns>
    /// A task that will complete with:
    /// <list type="bullet">
    ///     <item>
    ///         <description>A 'disposable' to be used for releasing the lock if operation finished succsessfully.</description>
    ///     </item>
    ///     <item>
    ///         <description>'Null' in case of exceeding the timeout.</description>
    ///     </item>
    /// </list>
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// If the <see cref="SemaphoreLock"/> is disposed.
    /// </exception>
    public async Task<IDisposable?> TryEnterAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var b = await _semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        if(b == false)
            return null;

        return new Releaser(_semaphore);
    }

    // Do not use Disposable mutable struct!
    // to avoid accidental copy and double release
    private sealed class Releaser: IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        // threadsafe bool, interlocked
        private volatile int _isDisposed = 0; //false;

        public Releaser(SemaphoreSlim semaphore)
        {
            ArgumentNullException.ThrowIfNull(semaphore);
            _semaphore = semaphore;
        }

        /// <summary>
        /// Release the lock
        /// Does not throw when <see cref="SemaphoreLock"/> is disposed.</br>
        /// </summary>
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