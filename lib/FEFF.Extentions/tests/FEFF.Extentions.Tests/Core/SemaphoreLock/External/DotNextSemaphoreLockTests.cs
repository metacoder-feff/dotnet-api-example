using DotNext.Threading;

namespace FEFF.Extentions.Tests.SemaphoreLock.External;

public class DotNextSemaphoreLockTests : IAsyncDisposable
{
    private readonly AsyncLock _loc = AsyncLock.Semaphore(1,1);
    public ValueTask DisposeAsync()
    {
        _loc.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public void Dispose_twice__should__not_throw()
    {
        _loc.Dispose();
        _loc.Dispose();
    }

    [Fact]
    public async Task Lock_twice__should__be_not_reentrant()
    {
        var l1 = await _loc.AcquireAsync(TestContext.Current.CancellationToken);
        l1.IsEmpty.Should().BeFalse();
        
        var l2 = await _loc.TryAcquireAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l2.IsEmpty.Should().BeTrue();
    }
    
    [Fact]
    public async Task Release_after_dispose__should__throw()
    {
        var l1 = await _loc.AcquireAsync(TestContext.Current.CancellationToken);
        l1.IsEmpty.Should().BeFalse();
        _loc.Dispose();

        var fn = () => l1.Dispose();
        fn.Should().Throw<ObjectDisposedException>();
    }
}