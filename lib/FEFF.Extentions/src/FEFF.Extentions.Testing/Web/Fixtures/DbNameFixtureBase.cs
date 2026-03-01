using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

/// <summary>
/// Mutates connectionstring - changes DB name.
/// TODO: remove db after test (without dbcontext)
/// </summary>
public class DbNameFixtureBase
{
    private readonly string _prefix;
    private readonly string _connectionStringName;
    private string? _oldCs;
    private string? _newCs;

    public DbNameFixtureBase(ITestApplicationFixture app, TestIdFixture testId, string connectionStringName)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionStringName);
//TODO: DRY
        _prefix = $"test-{testId.TestId}-";
        _connectionStringName = connectionStringName;

        app.ApplicationBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(WebHostBuilderContext ctx, IServiceCollection _)
    {
        var config = (ConfigurationManager)ctx.Configuration;
        var key = "ConnectionStrings:" + _connectionStringName;
        ChangeDbName(config, key);
    }
    
    private void ChangeDbName(ConfigurationManager config, string key)
    {
        var cs = config[key];
        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = cs
        };
        csb["Database"] = _prefix + csb["Database"];
        var newCs = csb.ConnectionString;
        config[key] = newCs;

        _oldCs = cs;
        _newCs = newCs;
    }

    // public async ValueTask DisposeAsync()
    // {
        // need to delete at all ?
        // or just leave ?
        // or intellegent backround batch delete after a number of tests finished?
        //await TryDeleteDatabaseAsync();
    // }
    
//TODO: delete without DbCtx
//     private async Task TryDeleteDatabaseAsync()
//     {
// //TODO: (optimization) check if app is started
//         // e.g. App cannot be started in a negative test
//         try
//         {
//             await DbCtx.Database.EnsureDeletedAsync();
//         }
//         catch { }
//     }
}