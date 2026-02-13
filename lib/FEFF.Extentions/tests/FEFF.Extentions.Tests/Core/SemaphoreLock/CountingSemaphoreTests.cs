namespace FEFF.Extentions.Tests.SemaphoreLock;

public class CountingSemaphoreTests : IDisposable
{
    private readonly FEFF.Extentions.SemaphoreLock _lock = new(5);

    public async void Dispose()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Handler_Dispose_twice__should__not_change__CurrentCount()
    {
        _lock.CurrentCount.Should().Be(5);
        var l1 = await _lock.EnterAsync(TestContext.Current.CancellationToken);
        _lock.CurrentCount.Should().Be(4);
        var l2 = await _lock.EnterAsync(TestContext.Current.CancellationToken);
        _lock.CurrentCount.Should().Be(3);

        l1.Dispose();
        _lock.CurrentCount.Should().Be(4);
        l1.Dispose();
        _lock.CurrentCount.Should().Be(4);
        
        l2.Dispose();
        _lock.CurrentCount.Should().Be(5);
    }
}