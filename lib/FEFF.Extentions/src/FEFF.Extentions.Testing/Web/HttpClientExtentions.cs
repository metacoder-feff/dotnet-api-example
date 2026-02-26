using System.Diagnostics;
using System.Net;
using System.Text;
using Xunit;

namespace FEFF.Extentions.Testing;

[DebuggerNonUserCode]
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
    
    public static async Task<string> TestPostAsync(this HttpClient client, string url, string? jsonData, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        var resp = await client.PostAsync(url, jsonData);
        var body = await resp.Content.ReadAsStringAsync();

        resp.StatusCode
            .Should().Be(expectedStatusCode, body);

        return body;
    }

    public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, string? jsonData = null)
    {
        if (jsonData == null)
        {
            return await client.PostAsync(url, null);
        }

        using var sc = new StringContent(jsonData, UnicodeEncoding.UTF8, "application/json");
        return await client.PostAsync(url, sc);
    }
}