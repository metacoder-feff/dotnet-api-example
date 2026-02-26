using System.Security.Claims;
namespace FEFF.Extentions.Web;
    
public interface IUserIdentityProvider
{
    string GetClaim(string claimType);
    string? TryGetClaim(string claimType);
}

public class HttpUserIdentityProvider : IUserIdentityProvider
{
    private readonly IHttpContextAccessor _accessor;

    public HttpUserIdentityProvider (IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string GetClaim(string claimType)
    {
        var httpContext = _accessor.HttpContext;
        ThrowHelper.Assert(httpContext != null);

        return httpContext.User.FindFirstValue(claimType) 
            ?? throw new InvalidOperationException($"Claim of type '{claimType}' not found.");
    }

    public string? TryGetClaim(string claimType)
    {
        return _accessor.HttpContext?.User?.FindFirstValue(claimType);
    }
}