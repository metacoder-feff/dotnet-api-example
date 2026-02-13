using DotNext.Threading;

namespace FEFF.Extentions.Tests.SemaphoreLock.External;

public class DotNextSemaphoreLockTests : IAsyncDisposable
{
    private readonly AsyncLock _lock = AsyncLock.Semaphore(1,1);
    public ValueTask DisposeAsync()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public void Dispose_twice__should__not_throw()
    {
        _lock.Dispose();
        _lock.Dispose();
    }

    [Fact]
    public async Task Lock_twice__should__be_not_reentrant()
    {
        var l1 = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        l1.IsEmpty.Should().BeFalse();
        
        var l2 = await _lock.TryAcquireAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l2.IsEmpty.Should().BeTrue();
    }
    
    [Fact]
    public async Task BUG__Release_after_dispose__throws()
    {
        var l1 = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        l1.IsEmpty.Should().BeFalse();
        _lock.Dispose();

        var fn = () => l1.Dispose();
        fn.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task DisposeAsync__nonreleased__should_finish()
    {
        var l = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        l.IsEmpty.Should().BeFalse();

        //act
        await _lock.DisposeAsync();
    }
    
    //BUG
    [Fact]
    public async Task BUG__AcquireAsync__when_disposing__CANNOT_return()
    {
        var l1 = await _lock.AcquireAsync(TestContext.Current.CancellationToken);
        l1.IsEmpty.Should().BeFalse();
        
        var t = Task.Run(
            async () => await _lock.AcquireAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken
        );

        // start TryEnterAsync before 'Act'
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Act 
        _lock.Dispose();

        // Assert
        var fn = async () => await t;
        await fn.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(5));
    }
}