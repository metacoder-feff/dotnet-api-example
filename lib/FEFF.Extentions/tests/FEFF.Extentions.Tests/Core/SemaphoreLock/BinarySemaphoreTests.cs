namespace FEFF.Extentions.Tests.SemaphoreLock;

public class BinarySemaphoreTests : IDisposable
{
    private readonly FEFF.Extentions.SemaphoreLock _lock = new();
    private readonly List<int> _list = [];

    public async void Dispose()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task Fn1()
    {
        // avoid race condition when updating '_list' in 'WithoutLock' mode
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
        using var l = await _lock.EnterAsync(TestContext.Current.CancellationToken);
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

    [Fact]
    public async Task DisposeAsync_twice__should__not_throw()
    {
        _lock.Dispose();
        _lock.Dispose();
    }

    [Fact]
    public async Task Dispose__nonreleased__should__not_cause_deadlock()
    {
        _ = await _lock.EnterAsync(TestContext.Current.CancellationToken);
        _lock.Dispose();
    }

    [Fact]
    public async Task Lock__IS_NOT_reentrant()
    {
        var l1 = await _lock.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l1.Should().NotBeNull();
        
        var l2 = await _lock.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l2.Should().BeNull();
    }

    [Fact]
    public async Task EnterAsync__when_disposed__should_throw()
    {
        _lock.Dispose();
        var fn = () => _lock.EnterAsync(TestContext.Current.CancellationToken);
        
        await fn.Should().ThrowExactlyAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task TryEnterAsync__when_disposed__should_throw()
    {
        _lock.Dispose();
        var fn = () => _lock.TryEnterAsync(TimeSpan.Zero, TestContext.Current.CancellationToken);
        
        await fn.Should().ThrowExactlyAsync<ObjectDisposedException>();
    }
    
    [Fact]
    public async Task Release__when_disposed__should__NOT_throw()
    {
        var l1 = await _lock.EnterAsync(TestContext.Current.CancellationToken);
        _lock.Dispose();

        var fn = () => l1.Dispose();
        fn.Should().NotThrow();
    }

    [Fact]
    public async Task TryEnterAsync_when_busy__should__return_null()
    {
        // PREPARE
        var l1 = await _lock.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l1.Should().NotBeNull();
        
        // ACT
        var l2 = await _lock.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        
        // ASSERT
        l2.Should().BeNull();
    }
    
    [Fact]
    public async Task TryEnterAsync_after_release__should__return__not_null()
    {
        // PREPARE
        var l1 = await _lock.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l1.Should().NotBeNull();
        
        var l2 = await _lock.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l2.Should().BeNull();

        // ACT
        l1.Dispose();

        // Assert
        var l3 = await _lock.TryEnterAsync(TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        l3.Should().NotBeNull();
    }

    [Fact]
    public async Task Handler_Dispose_twice__should__not_throw()
    {
        var l = await _lock.EnterAsync(TestContext.Current.CancellationToken);
        l.Dispose();
        l.Dispose();
    }
    
    [Fact]
    public async Task EnterAsync__while_disposing__should_throw()
    {
        var l = await _lock.EnterAsync(TestContext.Current.CancellationToken);

        var t = Task.Run(
            async () => await _lock.EnterAsync(TestContext.Current.CancellationToken),
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
    
    [Fact]
    public async Task TryEnterAsync__while_disposing__should_throw()
    {
        var l = await _lock.EnterAsync(TestContext.Current.CancellationToken);
        
        var t = Task.Run(
            async () => await _lock.TryEnterAsync(TimeSpan.FromSeconds(5), 
            TestContext.Current.CancellationToken)
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