using System.Net.Http.Headers;

namespace FEFF.Extentions;

public static class HttpClientExtentions
{
    // RFC 7235
    public const string BearerAuthHeader = "Bearer";

    public static void AddBearerHeader(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerAuthHeader, accessToken);
    }
}    