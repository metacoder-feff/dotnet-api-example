using System.Net;
using Xunit;

namespace FEFF.Extentions.Testing;

public static class HttpClientExtentions
{
    public static async Task<string> TestGetStringAsync(this HttpClient client, string requestUri, HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
//TODO: stopwatch
//TODO: FluentAseertions.Web?
        using var resp = await client.GetAsync(requestUri, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode
            .Should().Be(expectedStatus, body);
        return body;
    }
}