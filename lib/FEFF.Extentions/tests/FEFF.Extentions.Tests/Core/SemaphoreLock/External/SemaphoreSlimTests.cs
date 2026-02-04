namespace FEFF.Extentions.Tests.SemaphoreLock.External;

public class SemaphoreSlimTests : IAsyncDisposable
{
    private readonly SemaphoreSlim _sem = new (1);
    private SemaphoreSlim? _sem2;
    public ValueTask DisposeAsync()
    {
        _sem.Dispose();
        _sem2?.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public void Dispose_twice__should__not_throw()
    {
        _sem.Dispose();
        _sem.Dispose();
    }
    
    [Fact]
    public async Task Release_after_dispose__should__throw()
    {
        await _sem.WaitAsync(TestContext.Current.CancellationToken);
        _sem.Dispose();

        var fn = () => _sem.Release();
        fn.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task Lock_twice__should__be_not_reentrant()
    {
        await _sem.WaitAsync(TestContext.Current.CancellationToken);
        
        var b = await _sem.WaitAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);

        b.Should().BeFalse();
    }
    
    [Fact]
    public async Task ExtraRelease__when__unlimited_maxCount__should__increment_counter()
    {
        _sem.CurrentCount.Should().Be(1);

        var fn = () => _sem.Release();
        fn.Should().NotThrow();

        _sem.CurrentCount.Should().Be(2);
    }

    [Fact]
    public async Task ExtraRelease__when__limited_maxCount__should__throw()
    {
        _sem2 = new(1,1); 

        var fn = () => _sem2.Release();
        fn.Should().Throw<SemaphoreFullException>();
    }
}