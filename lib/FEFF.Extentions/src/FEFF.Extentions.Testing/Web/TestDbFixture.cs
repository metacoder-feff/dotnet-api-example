using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

/// <summary>
/// Mutates connectionstring - changes DB name.
/// TODO: remove db after test (without dbcontext)
/// </summary>
public class TestDbFixture
{
    private readonly string _newDbName;
    private readonly string _connectionStringName;

    public TestDbFixture(ITestApplicationBuilder appBuilder, string newDbName, string connectionStringName)
    {
        ArgumentException.ThrowIfNullOrEmpty(newDbName);
        ArgumentException.ThrowIfNullOrEmpty(connectionStringName);

        _newDbName = newDbName;
        _connectionStringName = connectionStringName;

        appBuilder.ConfigureServices(ReconfigureFactory);
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
//TODO: append suffix to existing name
        csb["Database"] = _newDbName;
        var newCs = csb.ConnectionString;
        config[key] = newCs;
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