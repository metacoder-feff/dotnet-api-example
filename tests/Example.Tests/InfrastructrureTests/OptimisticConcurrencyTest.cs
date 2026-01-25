using Microsoft.EntityFrameworkCore;

using Example.Api;

namespace Example.Tests.InfrastructrureTests;

public class OptimisticConcurrencyTest :ApiTestBase
{
    private const int ItemId = 1;

    [Fact]
    public async Task ConcurrentUpdate__should__throw()
    {
        // PREPARE
        await InitializeDbAsync();

        // ACT
        // first op starts
        var m = await GetForecastAsync();

        // second op starts and finishes
        await ModifyConcurrentlyAsync("333");

        // first op finishes (tested promise)
        var f = () => SetNameAsync(m, "222");

        //ASSERT 1: first op should not be finished
        await f.Should().ThrowAsync<DbUpdateConcurrencyException>();

        //ASSERT 2: second op is saved only
        await AssertItemAsync("333", 1);
    }

    private async Task<Forecast> GetForecastAsync()
    {
        return await DbCtx.Forecasts
            .AsNoTracking()
            .Where(x => x.Id == ItemId)
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    private async Task AssertItemAsync(string expectedName, long expectedCount)
    {
        var m = await GetForecastAsync();

        m.Name
            .Should().Be(expectedName);
        m.Count
            .Should().Be(expectedCount);
    }

    private async Task ModifyConcurrentlyAsync(string name)
    {
        var m = await GetForecastAsync();
        await SetNameAsync(m, name);
    }

    private async Task SetNameAsync(Forecast m, string name)
    {
        DbCtx.Attach(m);
        m.Name = name;
        m.Count++;
        await DbCtx.SaveChangesAsync(TestContext.Current.CancellationToken);
        DbCtx.ChangeTracker.Clear();
    }

    public async override ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await InitializeDbAsync();
    }

    private async Task InitializeDbAsync()
    {
        await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        DbCtx.Forecasts.Add(new Forecast { Id = ItemId, Name = "1111"});

        await DbCtx.SaveChangesAsync(TestContext.Current.CancellationToken);

        DbCtx.ChangeTracker.Clear();
    }

//TODO: generalize
    // public override async ValueTask DisposeAsync()
    // {
    //     await DbCtx.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
    //     await base.DisposeAsync();
    // }
}