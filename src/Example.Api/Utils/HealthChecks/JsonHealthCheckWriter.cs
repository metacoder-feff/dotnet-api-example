using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Utils.HealthChecks;

internal class JsonHealthCheckWriter
{
    private readonly Action<JsonSerializerOptions> _cfgAction;

    public JsonHealthCheckWriter(Action<JsonSerializerOptions> cfgAction)
    {
        ArgumentNullException.ThrowIfNull(cfgAction);
        _cfgAction = cfgAction;
    }

    public async Task WriteAsync(HttpContext context, HealthReport report)
    {
        var obj = new
        {
            Status   = report.Status,
            Duration = report.TotalDuration,
            // check = report.Entries.Value+name(==report.Entries.Key)
            Checks = report.Entries
                    .Select(e =>
                        new
                        {
                            Name        = e.Key,
                            Description = e.Value.Description,
                            Duration    = e.Value.Duration,
                            Status      = e.Value.Status,
                            Error       = e.Value.Exception?.Message,
                            Data        = e.Value.Data,
                            //Tags = e.Value.Tags,
                        }
                    )
                    .ToList()
        };

        // also can get action from IServiceProvider
        // but it used everywhere else (mvc, signal-r) as a delegate
        //var opts = context.RequestServices.GetRequiredJsonOptions();

        var opts = new JsonSerializerOptions();
        _cfgAction(opts);

        // TODO: CancellationToken ??
        await context.Response.WriteAsJsonAsync(obj, opts);
    }
}