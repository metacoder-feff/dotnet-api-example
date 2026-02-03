using System.Net.Http.Headers;

namespace System.Net.Http;

public static class HttpClientExtentions
{
    public static void AddJwtHeader(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}    