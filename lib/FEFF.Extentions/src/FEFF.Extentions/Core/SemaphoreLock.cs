namespace FEFF.Extentions;

//TODO: add TryGetLockAsync(Timeout) ?
/// <summary>
/// AsyncLock using 'SemaphoreSlim'. </br>
/// Does not throw on SemaphoreSlim.Release() is SemaphoreSlim is disposed.</br>
/// Does not wait for SemaphoreSlim.Release() in SemaphoreSlim.Dispose().
/// </summary>
/// <remarks>
/// NOT reentrant!!! <br/>
/// The nearest realization is DotNext.Threading.AsyncLock.Semahore 
/// but it throws at SemaphoreSlim.Release() 
/// and waits (may deadlock)  at AsyncLock.DisposeAsync().
/// </remarks>
public sealed class SemaphoreLock: IDisposable
{
    private readonly SemaphoreSlim _semaphore;// = new(ParrallelDBTests); // max locks on semaphore 

    public SemaphoreLock(int maxParallelism = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxParallelism, 0);
//TODO: maxCount?
        _semaphore = new (maxParallelism);
    }

//TODO: check double dispose
    public void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<Lock> EnterAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new Lock(_semaphore);
    }

    public async Task<Lock?> TryEnterAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var b = await _semaphore.WaitAsync(timeout, cancellationToken);
        if(b == false)
            return null;

        return new Lock(_semaphore);
    }

    public sealed class Lock: IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        // threadsafe bool, interlocked
        private volatile int _isDisposed = 0; //false;

        public Lock(SemaphoreSlim semaphore)
        {
            ArgumentNullException.ThrowIfNull(semaphore);
            _semaphore = semaphore;
        }

        //TODO: thread safe : test
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
            catch(ObjectDisposedException e)
            {
                var x = e.ToString();
                Console.WriteLine(x);
            }

            GC.SuppressFinalize(this);
        }
    }
}