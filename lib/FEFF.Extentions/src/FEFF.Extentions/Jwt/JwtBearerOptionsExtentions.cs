using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FEFF.Extentions.Jwt;

public static class JwtBearerOptionsExtentions
{
    /// <summary>
    /// Allow to pass token via url query string instead of as a header or a cookie (ex: /protected?access_token={TOKEN}).
    /// </summary>
    /// <remarks>
    /// However, in almost all cases it is recomended that you do not do this, as it comes with some security issues.<br/>
    /// If you perform a GET request with a JWT in the query param, it is possible that the browser will save the URL, which could lead to a leaked token.<br/>
    /// It is also very likely that your backend (such as nginx or uwsgi) could log the full url paths, which is obviously not ideal from a security standpoint.<br/>
    /// You must set short lifetime of a token via 'Exp' claim to minimize a risk of replay attack.
    /// </remarks>
    public static JwtBearerOptions AddQueryStringAuthentication(this JwtBearerOptions src, Func<PathString, bool>? predicate, string queryKey = "access_token")
    {
        var chained = src.Events.OnMessageReceived;

        src.Events.OnMessageReceived = context =>
        {
            OnMessageReceived(context, predicate, queryKey);
            return chained(context);
        };

        return src;
    }

    private static void OnMessageReceived(MessageReceivedContext context, Func<PathString, bool>? predicate, string queryKey)
    {
        if(predicate != null)
        {
            var r = predicate(context.Request.Path);
            if(r == false)
                return;
        }

        if (context.Request.Query.TryGetValue(queryKey, out var token))
            context.Token = token;
    }
}