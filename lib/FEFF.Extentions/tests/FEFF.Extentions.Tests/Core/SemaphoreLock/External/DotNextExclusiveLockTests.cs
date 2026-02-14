using DotNext.Threading;

namespace FEFF.Extentions.Tests.SemaphoreLock.External;

public class DotNextExclusiveLockTests : IDisposable
{
    private readonly AsyncLock _lock = AsyncLock.Exclusive();
    private AsyncLock.Holder _lockHolder = default;

    public void Dispose()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task DisposeAsync_twice__should__not_throw()
    {
        await _lock.DisposeAsync();
        await _lock.DisposeAsync();
    }

    [Fact]
    public void Dispose_twice__should__not_throw()
    {
        _lock.Dispose();
        _lock.Dispose();
    }

    //ATTENTION: 
    // AsyncLock.Exclusive.DisposeAsync
    // is 'Graceful' - it waits until all handlers to be released.
    [Fact]
    public async Task ATTENTION__DisposeAsync__nonreleased__should_not_return()
    {
        _lockHolder = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        _lockHolder.IsEmpty.Should().BeFalse();

        var fn = async () => await _lock.DisposeAsync();

        await fn.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Dispose__nonreleased__should_return()
    {
        _lockHolder = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        _lockHolder.IsEmpty.Should().BeFalse();

        _lock.Dispose();
    }

    [Fact]
    public async Task Lock_twice__should__be_not_reentrant()
    {
        _lockHolder = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        _lockHolder.IsEmpty.Should().BeFalse();
        
        var l2 = await _lock.TryAcquireAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l2.IsEmpty.Should().BeTrue();
    }
    
    //BUG
    [Fact]
    public async Task BUG__Release__after_dispose__throws()
    {
        var l1 = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        l1.IsEmpty.Should().BeFalse();
        _lock.Dispose();

        var fn = () => l1.Dispose();
        fn.Should().Throw<ObjectDisposedException>();
    }
    
    [Fact]
    public async Task AcquireAsync__when_disposing__should_throw()
    {
        var l1 = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        l1.IsEmpty.Should().BeFalse();
        
        var t = Task.Run(
            async () => await _lock.AcquireAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken
        );

        // start TryEnterAsync before 'Act'
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Act 
        _lock.Dispose();

        // Assert
        var fn = async () => await t;
        await fn.Should().ThrowExactlyAsync<ObjectDisposedException>();
    }
}