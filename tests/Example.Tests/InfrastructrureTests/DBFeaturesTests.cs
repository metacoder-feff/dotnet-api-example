using Microsoft.EntityFrameworkCore;

using Example.Api;

namespace Example.Tests.InfrastructrureTests;

public class DBFeaturesTests : ApiTestBase
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

    private async Task InitializeDbAsync()
    {
        await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var fc = new Forecast { Id = ItemId, Name = "1111"};
        DbCtx.Forecasts.Add(fc);

        await DbCtx.SaveChangesAsync(TestContext.Current.CancellationToken);

        DbCtx.ChangeTracker.Clear();
    }

    [Fact]
    public async Task CreatedAtUpdatedAt__should__be_set__on_create()
    {
        const string expected = "2000-01-01T00:00:00Z";

        // PREAPRE
        await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var fc = new Forecast();
        DbCtx.Forecasts.Add(fc);

        // PRE_ASSERT
        fc.CreatedAt.ToString()
            .Should().NotBeEquivalentTo(expected);
        fc.UpdatedAt.ToString()
            .Should().NotBeEquivalentTo(expected);

        // ACT
        await DbCtx.SaveChangesAsync(TestContext.Current.CancellationToken);

        // ASSERT: 1. set in place
        fc.UpdatedAt.ToString()
            .Should().BeEquivalentTo(expected);

        // ASSERT: 2. set in DB
        var m = await GetForecastAsync();
        m.CreatedAt.ToString()
            .Should().BeEquivalentTo(expected);
        m.UpdatedAt.ToString()
            .Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdatedAt__should__be_set__on_update()
    {
        const string expectedNew = "2000-01-09T13:45:00Z";

        // PREAPRE
        await CreatedAtUpdatedAt__should__be_set__on_create();
        DbCtx.ChangeTracker.Clear();
        
        // PRE_ASSERT
        var m1 = await GetForecastAsync();
        m1.UpdatedAt.ToString()
            .Should().BeEquivalentTo("2000-01-01T00:00:00Z");

        // ACT
        FakeTime.AdvanceMinutes(12345);
        DbCtx.Attach(m1);
        m1.Name = "xxx";
        await DbCtx.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // ASSERT: 1. set in place
        m1.UpdatedAt.ToString()
            .Should().BeEquivalentTo(expectedNew);

        // ASSERT: 2. set in DB
        var m2 = await GetForecastAsync();
        m2.UpdatedAt.ToString()
            .Should().BeEquivalentTo(expectedNew);
    }
}