namespace Example.Tests.InfrastructrureTests;

// Test statements needed to ensure most of other test results
public class SelfTests : ApiTestBase
{
    [Fact]
    public void FakeTime__should_be__set_before_test()
    {
        Factory.FakeTime.GetInstant().ToString()
            .Should().Be("2000-01-01T00:00:00Z");

        var tp = GetRequiredService<TimeProvider>();
        
        tp.GetInstant().ToString()
            .Should().Be("2000-01-01T00:00:00Z");
    }
    
    [Fact]
    public async Task TestDatabase__should_not__exist_before_test()
    {
        var b1 = await GetDbExistsAsync();
        b1.Should().BeFalse();

        // Assert the check is correct
        // after creation in should change status
        await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var b2 = await GetDbExistsAsync();
        b2.Should().BeTrue();
    }

    private async Task<bool> GetDbExistsAsync()
    {
        return await DbCtx.Database.CanConnectAsync(TestContext.Current.CancellationToken);
        // db.GetPendingMigrationsAsync
        // db.HasPendingModelChanges
    }
}