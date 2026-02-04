namespace FEFF.Extentions.Tests;

public class SemaphoreLockTests : IAsyncDisposable
{
    private readonly SemaphoreLock _loc = new();
    private readonly List<int> _list = [];

    public ValueTask DisposeAsync()
    {
        _loc.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    private async Task Fn1()
    {
        // avoid race condition when updating '_list'
        await Task.Delay(1000, TestContext.Current.CancellationToken);
        _list.Add(1);
    }

    private async Task Fn2()
    {
        // just async method
        await Task.Delay(0, TestContext.Current.CancellationToken);
        _list.Add(2);
    }
    
    private async Task WithoutLock(Func<Task> a)
    {
        await a();
    }

    private async Task WithLock(Func<Task> a)
    {
        using var l = await _loc.EnterAsync(TestContext.Current.CancellationToken);
        await a();
    }

    private async Task RunConcurrently(Func<Func<Task>, Task> runner)
    {
        var t1 = Task.Run( 
            async () => await runner(Fn1),
            TestContext.Current.CancellationToken
        );

        // wait task1 to enter the lock (when runner == Lock)
        await Task.Delay(100, TestContext.Current.CancellationToken);

        var t2 = Task.Run( 
            async () => await runner(Fn2),
            TestContext.Current.CancellationToken
        );

        await Task.WhenAll(t1, t2);
    }

    [Fact]
    public async Task Run_Without_Lock__should__mix_result()
    {
        await RunConcurrently(WithoutLock);

        _list.Should().BeEquivalentTo(
            [2,1]
        );
    }

    [Fact]
    public async Task Run_With_Lock__should__order_result()
    {
        await RunConcurrently(WithLock);

        _list.Should().BeEquivalentTo(
            [1,2]
        );
    }

//TODO: test w/o loc by order
    [Fact]
    public void DisposeAsync_twice__should__not_throw()
    {
        _loc.Dispose();
        _loc.Dispose();
    }

    [Fact]
    public async Task Dispose__nonreleased__should__not_cause_deadlock()
    {
        _ = await _loc.EnterAsync(TestContext.Current.CancellationToken);
        _loc.Dispose();
    }

    [Fact]
    public async Task Lock_twice__should__be_not_reentrant()
    {
        var l1 = await _loc.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l1.Should().NotBeNull();
        
        var l2 = await _loc.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l2.Should().BeNull();
    }
    
    [Fact]
    public async Task Release_after_dispose__should__NOT_throw()
    {
        var l1 = await _loc.EnterAsync(TestContext.Current.CancellationToken);
        _loc.Dispose();

        var fn = () => l1.Dispose();
        fn.Should().NotThrow();
    }

    [Fact]
    public async Task TryEnterAsync_when_busy__should__return_null()
    {
        // PREPARE
        var l1 = await _loc.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l1.Should().NotBeNull();
        
        // ACT
        var l2 = await _loc.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        
        // ASSERT
        l2.Should().BeNull();
    }
    
    [Fact]
    public async Task TryEnterAsync_when_release__should__return__not_null()
    {
        // PREPARE
        var l1 = await _loc.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l1.Should().NotBeNull();
        
        var l2 = await _loc.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l2.Should().BeNull();

        // ACT
        l1.Dispose();

        // Assert
        var l3 = await _loc.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l3.Should().NotBeNull();
    }
}