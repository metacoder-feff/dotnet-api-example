using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FEFF.Extentions.Jwt;

public static class JwtBearerOptionsExtentions
{
    /// <summary>
    // Allow to pass token via url query parameter.
    /// </summary>
    /// <remarks>
    /// You can also pass the token in as a paramater in the query string instead of as a header or a cookie (ex: /protected?access_token=<TOKEN>). 
    /// However, in almost all cases it is recomended that you do not do this, as it comes with some security issues. 
    /// If you perform a GET request with a JWT in the query param, it is possible that the browser will save the URL, which could lead to a leaked token. 
    /// It is also very likely that your backend (such as nginx or uwsgi) could log the full url paths, which is obviously not ideal from a security standpoint.
    /// </remarks>
    public static JwtBearerOptions AddQueryStringAuthentication(this JwtBearerOptions src, string queryKey = "access_token")
    {
//TODO: chain events
//TODO: filter routes
        src.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Query.TryGetValue(queryKey, out var token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };

        return src;
    }
}