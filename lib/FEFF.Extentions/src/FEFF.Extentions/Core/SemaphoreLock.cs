using System.Collections.Concurrent;

namespace FEFF.Extentions;

/*
The nearest realization is DotNext.Threading.AsyncLock: Semaphore/Exclusive.
But it throws at Release() when AsyncLock is disposed
and waits for all 'enters' to be released (may deadlock) at AsyncLock.DisposeAsync().
*/

//TODO: Think about reentrant realization based on AsyncLocal

/*
HINT: 
Do not implement 'auto dispose _semaphore after last release' feature because 
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
/// Throws ObjectDisposedException at EnterAsync/TryEnterAsync after SemaphoreLock is disposed.
/// </remarks>
public sealed class SemaphoreLock: IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    // awaiting _semaphore.WaitAsync() would not return after _semaphore.Dispose()
    // (because ManualResetEvent inside _semaphore would be silently disposed)
    // => use _disposingToken to return control to client from [Try]EnterAsync()
    private readonly CancellationToken _disposingToken;
    private readonly CancellationTokenSource _disposingCTS = new();
    private volatile bool _isDisposed = false;
    
    // avoid race condition between
    // 0. _isDisposed
    // 1. _disposingCTS.Cancel()
    // 2. _semaphore.WaitAsync()
    private readonly Lock _lockObj = new();

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
        _disposingToken = _disposingCTS.Token;
    }

    public void Dispose()
    {
        // double check (optimization)
        if (_isDisposed)
            return;

        lock(_lockObj)
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
                    
            // cancel pending [Try]EnterAsync requests
            _disposingCTS.Cancel();
        }

        _semaphore.Dispose();
        _disposingCTS.Dispose();

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
        // var r = await TryEnterAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
        // ThrowHelper.Assert(r is not null);// should never happen
        // return r;

        // double check (optimization)
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Task t;
        lock(_lockObj)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            t =_semaphore.WaitAsync(cancellationToken);
        }

        try
        {
//TODO: t.WaitAsync(_disposingToken) - safety of t?
            await t.WaitAsync(_disposingToken).ConfigureAwait(false);
            return new Releaser(_semaphore);
        }
        catch (OperationCanceledException e)
        when (e.CancellationToken == _disposingToken
            && cancellationToken.IsCancellationRequested == false)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            throw;
        }
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
        // double check (optimization)
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Task<bool> t;
        lock(_lockObj)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            t =_semaphore.WaitAsync(timeout, cancellationToken);
        }

        bool waitResult;
        try
        {
//TODO: t.WaitAsync(_disposingToken) - safety of t?
            waitResult = await t.WaitAsync(_disposingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException e)
        when (e.CancellationToken == _disposingToken
            && cancellationToken.IsCancellationRequested == false)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            throw;
        }

        if(waitResult == false)
            return null;

        return new Releaser(_semaphore);
    }

//TODO: ref struct like System.Threading.Lock.Scope
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