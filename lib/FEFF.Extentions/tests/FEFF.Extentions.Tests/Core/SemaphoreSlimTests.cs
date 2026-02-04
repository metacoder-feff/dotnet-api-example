namespace FEFF.Extentions.Tests;

public class SemaphoreSlimTests : IAsyncDisposable
{
    private readonly SemaphoreSlim _sem = new (1);
    public ValueTask DisposeAsync()
    {
        _sem.Dispose();
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
}