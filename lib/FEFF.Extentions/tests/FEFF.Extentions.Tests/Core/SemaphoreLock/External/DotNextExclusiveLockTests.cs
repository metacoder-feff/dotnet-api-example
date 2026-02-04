using DotNext.Threading;

namespace FEFF.Extentions.Tests.SemaphoreLock.External;

public class DotNextExclusiveLockTests : IAsyncDisposable
{
    private readonly AsyncLock _loc = AsyncLock.Exclusive();
    private AsyncLock.Holder _lockHolder = default;

    public async ValueTask DisposeAsync()
    {
        // release to avoid deadlock on _loc.DisposeAsync()
        if(_lockHolder.IsEmpty == false)
            _lockHolder.Dispose();

        await _loc.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task DisposeAsync_twice__should__not_throw()
    {
        await _loc.DisposeAsync();
        await _loc.DisposeAsync();
    }
    [Fact]
    public async Task DisposeAsync__nonreleased__should__cause_deadlock()
    {
        _lockHolder = await _loc.AcquireAsync(TestContext.Current.CancellationToken);
        _lockHolder.IsEmpty.Should().BeFalse();

        //var fn = () => _loc.DisposeAsync();
        // AwesomeAssertions does not work with 'ValueTask' for now
        
        async Task testedAction()
        {
            await _loc.DisposeAsync();
        }
        var fn = testedAction;

        await fn.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Lock_twice__should__be_not_reentrant()
    {
        _lockHolder = await _loc.AcquireAsync(TestContext.Current.CancellationToken);
        _lockHolder.IsEmpty.Should().BeFalse();
        
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